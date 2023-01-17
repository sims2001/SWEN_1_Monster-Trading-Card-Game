using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;


namespace MTCG_Rutschka {
    /// <summary>The Program Class for the application </summary>
    public static class Program {
        /// <summary> DataBase Handler for all Transactions </summary>
        internal static DbHandler MyDb;
        /// <summary> Dictionary to Handle all active Players </summary>
        internal static Dictionary<string, Player> ActivePlayers;
        /// <summary> A List with all Card IDs that are currently in the Game </summary>
        internal static List<string> AllCardIDs;
        /// <summary> CardShop Instance to buy and create Packages </summary>
        internal static CardShop Shop;
        /// <summary> TradeBank Instance to handle Trades</summary>
        internal static TradeBank TradeBank;
        /// <summary> Battle Instance to Handle all Battles</summary>
        internal static Battle BattleGround;

        /// <summary> Entry point -> Initiates the Server </summary>
        /// <param name="args"></param>
        static void Main(string[] args) {
            InitClasses();

            Console.WriteLine("Waiting for Requests!");

            MyHttpServer server = new MyHttpServer();
            server.Incoming += _Handle_Server_Request;
            server.Run();
        }

        /// <summary> Initiates all Needed Class Instances, Dictionaries and Lists </summary>
        internal static void InitClasses() {
            ActivePlayers = new Dictionary<string, Player>();
            AllCardIDs = new List<string>();
            Shop = new CardShop();
            TradeBank = new TradeBank();
            BattleGround = new Battle();

            MyDb = new DbHandler();
        }

        /// <summary> Alternative Constructor for the _Handle_Server_Request Function which Queues the Request in a new Thread </summary>
        /// <param name="sender">The Initiator of the Request</param>
        /// <param name="e"> An Instance of the ServerEventArgs Class with the request content </param>
        public static void _Handle_Server_Request(object sender, ServerEventArgs e) {
            ThreadPool.QueueUserWorkItem(_Handle_Server_Request, e);
        }


        /// <summary>
        /// Main Logic of the Program/REST API -> Uses a switch for the different Routes
        /// </summary>
        /// <param name="evt">An Instance of the ServerEventArgs Class with the request content</param>
        public static void _Handle_Server_Request(object evt) {
            ServerEventArgs e = (ServerEventArgs)evt;
            string usr, pwd;

            //Remove unneccessary parts from the Requested Path
            string pth = e.Path;
            string w = pth.Length >= 1 ? pth.Substring(1, pth.Length-1).ToLower() : "err";
            if (w.Contains("/"))
                w = w.Substring(0, w.IndexOf("/"));
            if (w.Contains("?"))
                w = w.Substring(0, w.IndexOf("?"));


            //The Reply whicht will be sent the user
            var status = 0;
            var statusMessage = "";
            var reply = "";


            //A try statemant to catch all "general" exceptions (Like Invalid Authorization)
            try {
                //Switch-Statement for the different Routes
                switch (w) {
                    case "users":
                        //Register new Users
                        if (e.Method == "POST") {
                            //Check Input
                            if (!e.KeyValueJson.HasValues || e.KeyValueJson.Count != 2)
                                throw new InvalidAuthorizationExceptions("NO, OR INVALID REGISTRATION-DATA SUBMITTED");

                            if (!e.KeyValueJson.ContainsKey("Username"))
                                throw new InvalidInputException("No username submitted");

                            if (!e.KeyValueJson.ContainsKey("Password"))
                                throw new InvalidInputException("No password submitted");


                            usr = e.KeyValueJson["Username"].ToString();
                            pwd = e.KeyValueJson["Password"].ToString();

                            if (usr.Length > 240 || usr.Length < 4)
                                throw new InvalidInputException("USERNAME HAS INVALID LENGTH");

                            if (pwd.Length > 250 || pwd.Length < 4)
                                throw new InvalidInputException("PASSWORD HAS INVALID LENGTH");

                            //Create User via DBHandler
                            try {
                                MyDb.CreateUser(usr, pwd);
                                status = 201;
                                statusMessage = "User successfully created";
                            }
                            catch (UserAlreadyExistsException) {
                                status = 409;
                                statusMessage = "User with same username already registered";
                            }

                        }
                        //Get User Information
                        else if (e.Method == "GET") {
                            //Authorize UserToken in DB and check Input
                            if (!MyDb.AuthenticateUser(e.Token))
                                throw new InvalidAuthorizationExceptions("Access token is missing or invalid");

                            usr = e.Path.Replace("/" + w, "");
                            if (usr.Contains("/"))
                                usr = usr.Replace("/", "").Trim();

                            if (String.IsNullOrEmpty(usr))
                                throw new InvalidInputException("No Username Submitted");

                            if (usr != e.Username && e.Username != "admin")
                                throw new InvalidAuthorizationExceptions("Insufficient Access-Rights");

                            //retrieve Userinformation from DBHandler
                            try {
                                reply += MyDb.GetUserInformation(usr);
                                status = 200;
                                statusMessage = "Data successfully retrieved";
                            }
                            catch (UserNotFoundException) {
                                status = 404;
                                statusMessage = "User not found";
                            }
                        }
                        //Update User Information
                        else if (e.Method == "PUT") {
                            //Authorize UserToken in DB and check Input
                            if (!MyDb.AuthenticateUser(e.Token))
                                throw new InvalidAuthorizationExceptions("Access token is missing or invalid");

                            usr = e.Path.Replace("/" + w, "");
                            if (usr.Contains("/"))
                                usr = usr.Replace("/", "").Trim();
                            if (String.IsNullOrEmpty(usr))
                                throw new InvalidInputException("No Username Submitted");

                            if (usr != e.Username && e.Username != "admin")
                                throw new InvalidAuthorizationExceptions("Insufficient Access-Rights");

                            if (e.KeyValueJson.Count != 3)
                                throw new InvalidInputException("Received invalid number of parameters");

                            if (!e.KeyValueJson.ContainsKey("Name") && !e.KeyValueJson.ContainsKey("Bio") &&
                                !e.KeyValueJson.ContainsKey("Image"))
                                throw new InvalidInputException("Empty parameters must be submitted with NULL values");

                            //Update UserInformation via DBHandler
                            try {
                                MyDb.UpdateUserInformation(usr, e.KeyValueJson["Name"].ToString(),
                                    e.KeyValueJson["Bio"].ToString(), e.KeyValueJson["Image"].ToString());
                                status = 200;
                                statusMessage = "User successfully updated";
                            }
                            catch (UserNotFoundException) {
                                status = 404;
                                statusMessage = "User not found";
                            }
                        }
                        else 
                            throw new NotImplementedYetException();

                        break;
                    //LogInUSers
                    case "sessions":
                        //Check for Request Method and validate Input
                        if (e.Method != "POST")
                            throw new NotImplementedYetException();

                        if (!e.KeyValueJson.ContainsKey("Username"))
                            throw new InvalidInputException("No Username submitted!");

                        if (!e.KeyValueJson.ContainsKey("Password"))
                            throw new InvalidInputException("No Password submitted!");

                        usr = e.KeyValueJson["Username"].ToString();
                        pwd = e.KeyValueJson["Password"].ToString();

                        if (usr.Length > 240 || usr.Length < 4)
                            throw new InvalidInputException("USERNAME HAS INVALID LENGTH");

                        if (pwd.Length > 250 || pwd.Length < 4)
                            throw new InvalidInputException("PASSWORD HAS INVALID LENGTH");

                        reply += MyDb.LoginUser(usr, pwd);
                        status = 200;
                        statusMessage = "User login successful";

                        break;
                    //Create Packages
                    case "packages":
                        //Check for Request Method and Authorize Admin
                        if (e.Method != "POST")
                            throw new NotImplementedYetException();

                        if (e.Username != "admin" || !MyDb.AuthenticateUser(e.Token))
                            throw new InvalidAuthorizationExceptions("Insufficient access-rights");

                        if (e.Username != "admin")
                            throw new NotAnAdminException();

                        Queue<ICard> newCards = new Queue<ICard>();

                        //Generate new Cards and Add them to Packets
                        try {
                            foreach (JObject c in e.DataJArray) {
                                if (!c.HasValues || c.Count != 3)
                                    throw new InvalidInputException("Wrong number of Arguments");

                                if (!c.ContainsKey("Id"))
                                    throw new InvalidInputException("No CardID submitted");

                                if (!c.ContainsKey("Name"))
                                    throw new InvalidInputException("No CardName submitted");


                                if (!c.ContainsKey("Damage"))
                                    throw new InvalidInputException("No CardDamage submitted");

                                if (AllCardIDs.Contains(c["Id"].ToString()))
                                    throw new CardAlreadyExistsException();

                                if (c["Name"].ToString().Contains("Spell"))
                                    newCards.Enqueue(new SpellCard(c["Id"].ToString(), c["Name"].ToString(),
                                        Convert.ToDouble(c["Damage"])));
                                else
                                    newCards.Enqueue(new MonsterCard(c["Id"].ToString(), c["Name"].ToString(),
                                        Convert.ToDouble(c["Damage"])));
                            }

                            //Check if Enough Cards
                            if (newCards.Count < 5)
                                throw new InvalidInputException("Not enough Cards to create Package");

                            //Generate PackID
                            var packId = Guid.NewGuid().ToString();

                            //Add all Cards to the DB
                            foreach (var c in newCards) {
                                AllCardIDs.Add(c.CardId);
                                MyDb.CreateCard(c);
                            }
                            
                            //Create Package and add it to the Shop and the DB
                            CardPackage p = new CardPackage(packId, newCards.Dequeue(), newCards.Dequeue(),
                                newCards.Dequeue(),
                                newCards.Dequeue(), newCards.Dequeue(), 5);

                            Shop.AddPackageToShop(p);
                            MyDb.CreatePackage(p);
                            status = 201;
                            statusMessage = "Package and cards successfully created";

                        }
                        catch (InvalidInputException exc) {
                            status = 418;
                            statusMessage = exc.Message;
                        }
                        catch (CardAlreadyExistsException) {
                            status = 409;
                            statusMessage = "Not enough Cards to create Package";
                        }
                        finally {
                            newCards.Clear();
                        }
                        
                        break;
                    //Buy Packs
                    case "transactions":
                        //Check for Request Method, Authorize User and Check Input
                        if (e.Method != "POST")
                            throw new NotImplementedYetException();

                        if (!MyDb.AuthenticateUser(e.Token))
                            throw new InvalidAuthorizationExceptions("Access token is missing or invalid");

                        if (!e.Path.EndsWith("/packages"))
                            throw new InvalidInputException("No transaction-type submitted");

                        //User buys card from Shop and Updates PlayerStats (less Money) via DBHandler
                        try {
                            Shop.BuyCardPackage(ActivePlayers[e.Username]);
                            MyDb.UpdateUserStats(ActivePlayers[e.Username]);
                            status = 200;
                            statusMessage = "A package has been successfully bought";
                        }
                        catch (NoCardPackageException) {
                            status = 404;
                            statusMessage = "No card package available for buying";
                        }
                        catch (NotEnoughMoneyException) {
                            status = 403;
                            statusMessage = "Not enough money for buying a card package";
                        }
                        break;
                    //Get All cards of User
                    case "cards":
                        //Check for Request Method and Authorize User
                        if (e.Method != "GET")
                            throw new NotImplementedYetException();

                        if (!MyDb.AuthenticateUser(e.Token))
                            throw new InvalidAuthorizationExceptions("Access token is missing or invalid");

                        //Return Cards in Stack
                        try {
                            reply = ActivePlayers[e.Username].GetStackCards();
                            status = 200;
                            statusMessage = "Successfully requested user cards";
                        }
                        catch (NoCardsException) {
                            status = 204;
                            statusMessage = "The request was fine, but the user doesn't have any cards";
                        }
                        break;
                    //Get all cards in Deck of User
                    case "deck":
                        //Check for Request Method and Authorize User
                        if (e.Method != "GET" && e.Method != "PUT")
                            throw new NotImplementedYetException();

                        if (!MyDb.AuthenticateUser(e.Token))
                            throw new InvalidAuthorizationExceptions("Access token is missing or invalid");

                        //Return UserDeck -> PlainText or JSon(standard)
                        if (e.Method == "GET") {
                            try {
                                //Check if format is submitted
                                if (e.Path.Contains("?")) {
                                    var format = e.Path.Substring(e.Path.IndexOf("?")+1, 6);
                                    if(format != "format")
                                        throw new NotImplementedYetException();

                                    format = e.Path.Substring(e.Path.IndexOf("=") + 1);
                                    if (format == "plain")
                                        reply += ActivePlayers[e.Username].GetDeckCards();
                                    else
                                        reply += ActivePlayers[e.Username].GetDeckCardsJson();
                                } 
                                else
                                    reply += ActivePlayers[e.Username].GetDeckCardsJson();

                                status = 200;
                                statusMessage = "Successfully read Playerdeck";
                            }
                            catch (NoCardsException) {
                                status = 204;
                                statusMessage = "The request was fine, but the user doesn't have any cards";
                            }
                            catch (NotImplementedYetException) {
                                status = 501;
                                statusMessage = "Parameter not Implemented";
                            }
                        }
                        //Put cards to Deck
                        else {
                            try {
                                //Validate Input (for cards that need to be in the Stack)
                                if (e.SingleValues.Length != 4)
                                    throw new WrongCardCountException();

                                if (ActivePlayers[e.Username].NotAllInStack(e.SingleValues))
                                    throw new CardNotAvailableException();

                                foreach (var card in e.SingleValues) {
                                    ActivePlayers[e.Username].MoveToDeck(card);
                                }

                                status = 200;
                                statusMessage = "The deck has been successfully configured";
                            }
                            catch (WrongCardCountException) {
                                status = 400;
                                statusMessage = "The provided deck did not include the required amount of cards";
                            }
                            catch (CardNotAvailableException) {
                                status = 403;
                                statusMessage = "At least one of the provided cards does not belong to the user or is not available";
                            }
                        }

                        break;
                    //Battle Users
                    case "battles":
                        //Check for Request Method and Authorize User
                        if (e.Method != "POST")
                            throw new NotImplementedYetException();

                        if (!MyDb.AuthenticateUser(e.Token))
                            throw new InvalidAuthorizationExceptions("Access token is missing or invalid");

                        //Add Player To Fight Queue and wait for Other fighter
                        try {
                            reply += BattleGround.WantsToFight(ActivePlayers[e.Username]);
                            status = 200;
                            statusMessage = "The battle has been carried out successfully";
                        }
                        catch (WrongCardCountException) {
                            status = 418;
                            statusMessage = "Not enough Cards in Playerdeck";
                        }
                        catch (AlreadyInQueueException) {
                            status = 405;
                            statusMessage = "User already in Queue";
                        }

                        break;
                    //Return player stats
                    case "stats":
                        //Check for Request Method and Authorize User
                        if (e.Method != "GET")
                            throw new NotImplementedYetException();

                        if (!MyDb.AuthenticateUser(e.Token))
                            throw new InvalidAuthorizationExceptions("Access token is missing or invalid");

                        //Get User Stats from DBHandler as json
                        try {
                            reply += MyDb.GetUserStats(e.Username);
                            status = 200;
                            statusMessage = "The stats have been retrieved successfully";
                        }
                        catch (UserNotFoundException) {
                            status = 404;
                            statusMessage = "User not found";
                        }
                        break;
                    //Get Scoreboard
                    case "score":
                        //Check for Request Method and Authorize User
                        if (e.Method != "GET")
                            throw new NotImplementedYetException();

                        if (!MyDb.AuthenticateUser(e.Token))
                            throw new InvalidAuthorizationExceptions("Access token is missing or invalid");

                        //Get stats for all Users in DBHandler as json
                        try {
                            reply += MyDb.GetScoreBoard();
                            status = 200;
                            statusMessage = "The scoreboard has successfully been retrieved";
                        }
                        catch (UserNotFoundException)  {
                            status = 404;
                            statusMessage = "User not found";
                        }
                        break;
                    //TradeHandling
                    case "tradings":
                        //Check for Request Method and Authorize User
                        if (e.Method != "GET" && e.Method != "POST" && e.Method != "DELETE")
                            throw new NotImplementedYetException();

                        if (!MyDb.AuthenticateUser(e.Token))
                            throw new InvalidAuthorizationExceptions("Access token is missing or invalid");

                        //Return all Trades as json
                        if (e.Method == "GET") {
                            try {
                                reply += TradeBank.GetTrades();
                                status = 200;
                                statusMessage = "There are trading deals available";
                            }
                            catch (NoTradingDealException) {
                                status = 204;
                                statusMessage = "The request was fine, but there are no trading deals available";
                            }
                        }

                        if (e.Method == "POST") {
                            //Create new Trade
                            if (e.Path.Equals("/tradings")) {
                                //Validate Input
                                if (!e.KeyValueJson.ContainsKey("Id"))
                                    throw new InvalidInputException("No TradeID submitted");

                                if (!e.KeyValueJson.ContainsKey("CardToTrade"))
                                    throw new InvalidInputException("No Card To Trade submitted");

                                CardType type = CardType.None;
                                ElementType elem = ElementType.None;
                                double dmg = 0.0;
                                if (e.KeyValueJson.ContainsKey("Type")) {
                                    type = e.KeyValueJson["Type"].ToString() == "monster"
                                        ? CardType.Monster
                                        : CardType.Spell;
                                }

                                if (e.KeyValueJson.ContainsKey("Element")) {
                                    switch (e.KeyValueJson["Element"].ToString()) {
                                        case "fire":
                                            elem = ElementType.Fire;
                                            break;
                                        case "water":
                                            elem = ElementType.Water;
                                            break;
                                        case "normal":
                                            elem = ElementType.Normal;
                                            break;
                                    }
                                }

                                if (e.KeyValueJson.ContainsKey("MinimumDamage")) {
                                    dmg = Convert.ToDouble(e.KeyValueJson["MinimumDamage"]);
                                }

                                string trdId = e.KeyValueJson["Id"].ToString();
                                string trdCardId = e.KeyValueJson["CardToTrade"].ToString();
                                Player trader = ActivePlayers[e.Username];
                                ICard trdCard = ActivePlayers[e.Username].RemoveTradeCard(trdCardId);

                                //Register Trade in tradebank
                                try {
                                    if (trdCard == null)
                                        throw new CardNotAvailableException();

                                    TradeBank.RegisterTrade(trdId, trader, trdCard, type, elem, dmg);
                                    status = 201;
                                    statusMessage = "Trading deal successfully created";
                                }
                                catch (CardNotAvailableException) {
                                    status = 403;
                                    statusMessage = "The deal contains a card that is not owned by the user or locked in the deck";
                                }
                                catch (TradeAlreadyExistsException) {
                                    status = 409;
                                    statusMessage = "A deal with this tradeID already exists";
                                }
                            }
                            else {
                                //Extract Path to Trade (Trade from list)
                                if (e.SingleValues.Length != 1)
                                    throw new InvalidInputException("No TradeID submitted or in wrong format");
                                string toTrade = e.SingleValues[0];

                                //Trade cards
                                try {
                                    string extractedTradeId = e.Path.Replace("/tradings/", "").Replace("/", "").Trim();
                                    ICard traderCard = ActivePlayers[e.Username].OfferTradeCard(toTrade);
                                    if (traderCard == null)
                                        throw new InvalidTradeCardException();
                                    TradeBank.Trade(ActivePlayers[e.Username], traderCard, extractedTradeId);
                                    status = 200;
                                    statusMessage = "Trading deal successfully executed";
                                }
                                catch (InvalidTradeCardException) {
                                    status = 403;
                                    statusMessage =
                                        "The offered card is not owned by the user, or the requirements are not met";
                                }
                                catch (SelfTradeException) {
                                    status = 418;
                                    statusMessage = "Cannot trade with yourself";
                                }
                                catch (NoTradingDealException) {
                                    status = 404;
                                    statusMessage = "The provided tradeId was not found";
                                }
                            }
                        }
                        //Delete trade
                        if (e.Method == "DELETE") {
                            //Try to delete submitted Trade
                            try {
                                string extractedId = e.Path.Replace("/tradings/", "").Replace("/", "").Trim();
                                Console.WriteLine($"Trying to Delete Trade with ID: '{extractedId}'");
                                TradeBank.DeleteTrade(extractedId, ActivePlayers[e.Username]);
                                status = 200;
                                statusMessage = "Trading deal successfully deleted";
                            }
                            catch (InvalidTradeCardException) {
                                status = 403;
                                statusMessage = "The deal contains a card that is not owned by the user";
                            }
                            catch (NoTradingDealException) {
                                status = 404;
                                statusMessage = "The provided TradeID was not found";
                            }
                        }

                        break;
                    default:
                        throw new NotImplementedYetException();
                }
            }
            //Catch Exceptions
            catch (InvalidAuthorizationExceptions exc) {
                status = 401;
                statusMessage = exc.Message;
            }
            catch (InvalidInputException exc) {
                status = 418;
                statusMessage = exc.Message;
            }
            catch (NullReferenceException) {
                status = 418;
                statusMessage = "No or Wrong Parameters";
            }
            catch (NotImplementedYetException) {
                status = 501;
                statusMessage = "Not Implemented";
            }
            catch (NotAnAdminException) {
                status = 403;
                statusMessage = "Provided user is not 'admin'";
            }

            //Send Reply to Client
            e.ServerReply(status, statusMessage, reply);
        }

    }
}
