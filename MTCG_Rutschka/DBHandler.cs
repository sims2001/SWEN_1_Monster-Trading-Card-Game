using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Npgsql;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MTCG_Rutschka {

    /// <summary> a Handler to perform all DB-related tasks </summary>
    class DbHandler {

        private static string _cs;

        /// <summary> The Constructor for the DBHandler Creates all needed Tables and Initiates all available Data from said Tables </summary>
        public DbHandler() {
            _cs = "Host=localhost;Username=postgres;Password=s$cret;Database=postgres";

            _CreateNeededDatabases();

            _cardsToInitiate = new Dictionary<string, ICard>();
            _INITIATE_USERS();
            _INITIATE_CARDS();
            _INITIATE_SHOP();
            _INITIATE_STACKS();
            _INITIATE_DECKS();
            _INITIATE_TRADEBANK();
            _INITIATE_BATTLEHISTORY();
            Console.WriteLine("SUCCESSFULLY INITIATED DBs");
        }

        //PUBLIC VARIABLES AND FUNCTIONS
        //USER HANDLING
        /// <summary> Creates A User in the 'users' Table with the Username and Password received </summary>
        /// <param name="username"> Username to register </param>
        /// <param name="pwd"> Password to register </param>
        /// <exception cref="UserAlreadyExistsException"> Thrown when User already Exists </exception>
        public void CreateUser(string username, string pwd) {
            if (_UserExists(username)) 
                throw new UserAlreadyExistsException("User with same username already registered");

            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            var token = username + "-mtcgToken";
            var hashedPwd = BCrypt.Net.BCrypt.HashPassword(pwd);
            var createUserCommand = "INSERT INTO users(username, password, token) VALUES(@usn, @pw, @tok);"
                                        + "INSERT INTO ranking(username) VALUES(@usn);";
            using var cmd = new NpgsqlCommand(createUserCommand, localConnection);
            cmd.Parameters.AddWithValue("usn", username);
            cmd.Parameters.AddWithValue("pw", hashedPwd);
            cmd.Parameters.AddWithValue("tok", token);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            Program.ActivePlayers.Add(username, new Player(username, 20, 100, 0, 0, 0, 0));
        }

        /// <summary>Checks if the UserToken can be found in the DataBase</summary>
        /// <param name="token">Submitted UserToken </param>
        /// <returns> True or false if token found in DB</returns>
        public bool AuthenticateUser(string token) {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            bool auth = false;
            var tok = token.Replace("Basic ", "");
            using var cmd = new NpgsqlCommand("SELECT token FROM users WHERE token = @token", localConnection);
            cmd.Parameters.AddWithValue("token", tok);
            cmd.Prepare();
            using var rdr = cmd.ExecuteReader();
            if (rdr.HasRows) {
                auth = true;
            }
            rdr.Close();

            return auth;
        }

        /// <summary> Searches for the User in the DB, verifies the password and returns the according token </summary>
        /// <param name="user"> Username to login </param>
        /// <param name="pwd"> password to login </param>
        /// <exception cref="InvalidAuthorizationExceptions">Thrown when the user is not found or the password is incorrect </exception>
        /// <returns></returns>
        public string LoginUser(string user, string pwd) {
            string token;
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("SELECT password, token FROM users WHERE username = @un", localConnection);
            cmd.Parameters.AddWithValue("un", user);
            cmd.Prepare();
            using var rdr = cmd.ExecuteReader();
            if (rdr.HasRows) {
                rdr.Read();
                if (BCrypt.Net.BCrypt.Verify(pwd, rdr.GetString(0)))
                    token = rdr.GetString(1);
                else
                    throw new InvalidAuthorizationExceptions("Invalid password provided");

            } else
                throw new InvalidAuthorizationExceptions("Invalid username provided");
            
            return token;
        }

        /// <summary> Update the Stats of the Player in the Database </summary>
        /// <param name="p"> Player to update </param>
        public void UpdateUserStats(Player p) {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();

            string updateUsr = "UPDATE users SET coins = @c WHERE username = @un;" +
                              "UPDATE ranking SET points = @p, gamecount = @gc, wins = @w, losses = @l, ties = @t WHERE username = @un;";

            using var cmd = new NpgsqlCommand(updateUsr, localConnection);
            cmd.Parameters.AddWithValue("c", p.PlayerCoins);
            cmd.Parameters.AddWithValue("un", p.PlayerName);
            cmd.Parameters.AddWithValue("p", p.Elo);
            cmd.Parameters.AddWithValue("gc", p.PlayerGc);
            cmd.Parameters.AddWithValue("w", p.PlayerWin);
            cmd.Parameters.AddWithValue("l", p.PlayerLoss);
            cmd.Parameters.AddWithValue("t", p.PlayerTie);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        /// <summary> Update userInformation in the DB </summary>
        /// <param name="usr"> User to update </param>
        /// <param name="name"> new realname value </param>
        /// <param name="bio"> new bio value </param>
        /// <param name="img"> new image value </param>
        /// <exception cref="UserNotFoundException"> Thrown when there User to Update doesn't exist </exception>
        public void UpdateUserInformation(string usr, string name, string bio, string img) {
            if (!_UserExists(usr))
                throw new UserNotFoundException();

            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            string updateCommand = "UPDATE users SET realname = @rn, bio = @bio, image = @img WHERE username = @usr;";
            using var cmd = new NpgsqlCommand(updateCommand, localConnection);
            cmd.Parameters.AddWithValue("rn", name);
            cmd.Parameters.AddWithValue("bio", bio);
            cmd.Parameters.AddWithValue("img", img);
            cmd.Parameters.AddWithValue("usr", usr);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        /// <summary> Returns a JSonString with the Users Stats </summary>
        /// <param name="user"> User to get the stats from </param>
        /// <exception cref="UserNotFoundException"> Thrown when there UserStats requested don't exist </exception>
        /// <returns> JSonString with UserStats </returns>
        public string GetUserStats(string user) {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();

            using var cmd = new NpgsqlCommand("SELECT * FROM ranking WHERE username = @user;", localConnection);
            cmd.Parameters.AddWithValue("user", user);
            cmd.Prepare();
            using var rdr = cmd.ExecuteReader();

            if (rdr.HasRows)
                while (rdr.Read())
                    return JsonSerializer.Serialize(new UserStatsSchema(rdr.GetString(1), rdr.GetInt32(2), rdr.GetInt32(4), rdr.GetInt32(5), rdr.GetInt32(6)));
            else
                throw new UserNotFoundException();

            return null;
        }

        /// <summary> Returns a JSonString with the Users Information </summary>
        /// <param name="user"> User to get the information from </param>
        /// <exception cref="UserNotFoundException"> Thrown when the UserInformation requested don't exist </exception>
        /// <returns> JSonString with UserInformation </returns>
        public string GetUserInformation(string user) {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();

            using var cmd = new NpgsqlCommand("SELECT realname, bio, image FROM users WHERE username = @user;", localConnection);
            cmd.Parameters.AddWithValue("user", user);
            cmd.Prepare();
            using var rdr = cmd.ExecuteReader();

            string userInformation = null;
            if (rdr.HasRows)
                while (rdr.Read())
                    userInformation =
                        JsonSerializer.Serialize(new UserDataSchema(rdr.GetString(0), rdr.GetString(1),
                            rdr.GetString(2)));
            else
                throw new UserNotFoundException();

            rdr.Close();

            return userInformation;
        }

        /// <summary> Returns a JSonArrayString with all the Users Stats </summary>
        /// <exception cref="UserNotFoundException"> Thrown when there are no UserStats found </exception>
        /// <returns> JSonArrayString with all UserStats </returns>
        public string GetScoreBoard() {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();

            using var cmd = new NpgsqlCommand("SELECT * FROM ranking ORDER BY points DESC", localConnection);
            using var rdr = cmd.ExecuteReader();
            List<UserStatsSchema> scoreBoard = new List<UserStatsSchema>();

            if (rdr.HasRows) 
                while (rdr.Read())
                    scoreBoard.Add(new UserStatsSchema(rdr.GetString(1), rdr.GetInt32(2), rdr.GetInt32(4), rdr.GetInt32(5), rdr.GetInt32(6)));
            else
                throw new UserNotFoundException();

            return JsonConvert.SerializeObject(scoreBoard);
        }
        //BATTLE HANDLING

        /// <summary> Inserts a new BattleResult to the DB </summary>
        /// <param name="battle">the BatleResult to insert</param>
        public void CreateBattle(BattleResult battle) {
            var winner = battle.TieBattle ? "TIE" : battle.Winner.PlayerName;
            var loser = battle.TieBattle ? "TIE" : battle.Loser.PlayerName;
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();

            using var cmd = new NpgsqlCommand("INSERT INTO battleresults(battleid, player_one, player_two, tie, winner, loser) VALUES (@bid, @pone, @ptwo, @tie, @win, @los);", localConnection);
            cmd.Parameters.AddWithValue("bid", battle.BattleId);
            cmd.Parameters.AddWithValue("pone", battle.PlayerOne.PlayerName);
            cmd.Parameters.AddWithValue("ptwo", battle.PlayerTwo.PlayerName);
            cmd.Parameters.AddWithValue("tie", battle.TieBattle);
            cmd.Parameters.AddWithValue("win", winner);
            cmd.Parameters.AddWithValue("los", loser);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            foreach (var round in battle.BattleRounds) {
                CreateBattleRound(battle.BattleId, round);
            }
        }
        
        /// <summary> Insert a new BattleRound to the DB </summary>
        /// <param name="battleId">the Battle's ID the Round 'belongs' to </param>
        /// <param name="round"> BattleRound Instance to Insert </param>
        private static void CreateBattleRound(string battleId, BattleRound round) {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("INSERT INTO battlerounds(battleid, roundnum, tie, winner, loser, winnercard, losercard)"
                + "VALUES(@bid, @rn, @tie, @win, @los, @wincard, @loscard);" , localConnection);
            cmd.Parameters.AddWithValue("bid", battleId);
            cmd.Parameters.AddWithValue("rn", round.RoundNum);
            cmd.Parameters.AddWithValue("tie", round.Tie);
            cmd.Parameters.AddWithValue("win", round.RoundWinner.PlayerName);
            cmd.Parameters.AddWithValue("los", round.RoundLoser.PlayerName);
            cmd.Parameters.AddWithValue("wincard", round.WinnerCard.CardId);
            cmd.Parameters.AddWithValue("loscard", round.LoserCard.CardId);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }


        //CARD HANDLING
        /// <summary> Insert a new Card to the DB </summary>
        /// <param name="card">Card to Insert </param>
        public void CreateCard(ICard card) {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();

            string createCardCommand = "INSERT INTO cards(cardid, cardname, carddamage) VALUES (@id, @name, @damage);";
            using var cmd = new NpgsqlCommand(createCardCommand, localConnection);
            cmd.Parameters.AddWithValue("id", card.CardId);
            cmd.Parameters.AddWithValue("name", card.CardName);
            cmd.Parameters.AddWithValue("damage", card.CardDamage);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        /// <summary> Inserts a new CardPackage to the DB </summary>
        /// <param name="pack"> The CardPackage to insert </param>
        public void CreatePackage(CardPackage pack) {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            string[] ids = pack.GetCardIDs();
            using var cmd = new NpgsqlCommand("INSERT INTO packs(packid, firstc, secondc, thirdc, fourthc, fifthc) VALUES (@id, @co, @ct, @cth, @cf, @cfi);", localConnection);
            cmd.Parameters.AddWithValue("id", pack.PacketId);
            cmd.Parameters.AddWithValue("co", ids[0]);
            cmd.Parameters.AddWithValue("ct", ids[1]);
            cmd.Parameters.AddWithValue("cth", ids[2]);
            cmd.Parameters.AddWithValue("cf", ids[3]);
            cmd.Parameters.AddWithValue("cfi", ids[4]);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        /// <summary> Deletes a Package from the DB </summary>
        /// <param name="p"> Package to delete </param>
        public void PackageSold(CardPackage p) {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("DELETE FROM packs WHERE packid = @pack", localConnection);
            cmd.Parameters.AddWithValue("pack", p.PacketId);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        /// <summary> Adds A CardID to the Users Stack </summary>
        /// <param name="name"> the username the cards added to </param>
        /// <param name="c"> the Card added </param>
        public void PackToStack(string name, ICard c) {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("INSERT INTO stacks(username, cardid) VALUES(@un, @card);", localConnection);
            cmd.Parameters.AddWithValue("un", name);
            cmd.Parameters.AddWithValue("card", c.CardId);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        /// <summary> Deletes a card from Users stack and inserts it to the users deck </summary>
        /// <param name="name"> Username for transaction </param>
        /// <param name="c"> card to delete/insert </param>
        public void StackToDeck(string name, ICard c) {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("DELETE FROM stacks WHERE cardid = @card; INSERT INTO decks(username, cardid) VALUES(@un, @card);", localConnection);
            cmd.Parameters.AddWithValue("un", name);
            cmd.Parameters.AddWithValue("card", c.CardId);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        /// <summary> Deletes a card from Users deck and inserts it to the users stack </summary>
        /// <param name="name"> Username for transaction </param>
        /// <param name="c"> card to delete/insert </param>
        public void DeckToStack(string name, ICard c) {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("DELETE FROM decks WHERE cardid = @card; INSERT INTO stacks(username, cardid) VALUES(@un, @card);", localConnection);
            cmd.Parameters.AddWithValue("un", name);
            cmd.Parameters.AddWithValue("card", c.CardId);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        /// <summary> Function called during Battle -> Transfers Card between users </summary>
        /// <param name="to"> New Players name </param>
        /// <param name="c"> the Card to change </param>
        public void ChangeDeckPlayers(string to, ICard c) {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("UPDATE decks SET username = @to WHERE cardid = @card", localConnection);
            cmd.Parameters.AddWithValue("to", to);
            cmd.Parameters.AddWithValue("card", c.CardId);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        /// <summary> Insert a trade in the DB and remove the corresponding card from the users stack </summary>
        /// <param name="theTrade"> The Trade to register in the DB </param>
        public void RegisterTrade(Trade theTrade) {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("DELETE FROM stacks WHERE cardid = @cid;" +
                                              "INSERT INTO tradebank(tradeid, seller, cardid, reqtype, reqelement, reqdmg) VALUES(@tid, @trd, @cid, @typ, @el, @dmg);", localConnection);
            cmd.Parameters.AddWithValue("tid", theTrade.TradeId);
            cmd.Parameters.AddWithValue("trd", theTrade.Vendor.PlayerName);
            cmd.Parameters.AddWithValue("cid", theTrade.OfferedCard.CardId);
            cmd.Parameters.AddWithValue("typ", theTrade.TypeString());
            cmd.Parameters.AddWithValue("el", theTrade.ElementString());
            cmd.Parameters.AddWithValue("dmg", theTrade.RequestedDamage);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        /// <summary> Delete a Trade from the DB and add the corresponding Card to the traders Stack </summary>
        /// <param name="theTrade"> The Trade to delete </param>
        public void WithdrawTrade(Trade theTrade) {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("DELETE FROM tradebank WHERE tradeid = @tid;" +
                                              "INSERT INTO stacks(username, cardid) VALUES(@un, @cid);", localConnection);
            cmd.Parameters.AddWithValue("tid", theTrade.TradeId);
            cmd.Parameters.AddWithValue("un", theTrade.Vendor.PlayerName);
            cmd.Parameters.AddWithValue("cid", theTrade.OfferedCard.CardId);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        /// <summary> Updates both Players Stacks and deletes the Trade  </summary>
        /// <param name="theTrade"> The trade to delete </param>
        /// <param name="tradeCard"> The Card to Trade </param>
        /// <param name="buyer"> The player who trades </param>
        public void Trade(Trade theTrade, ICard tradeCard, Player buyer) {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("DELETE FROM tradebank WHERE tradeid = @tid;" +
                                              "INSERT INTO stacks(username, cardid) VALUES (@buy, @trdcid);" +
                                              "UPDATE stacks SET username = @trd WHERE cardid = @newcid;", localConnection);
            cmd.Parameters.AddWithValue("tid", theTrade.TradeId);
            cmd.Parameters.AddWithValue("buy", buyer.PlayerName);
            cmd.Parameters.AddWithValue("trdcid", theTrade.OfferedCard.CardId);
            cmd.Parameters.AddWithValue("trd", theTrade.Vendor.PlayerName);
            cmd.Parameters.AddWithValue("newcid", tradeCard.CardId);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        //PRIVATE VARIABLES
        /// <summary> Dictionary including all Cards that need to be initiated </summary>
        private Dictionary<string, ICard> _cardsToInitiate;

        /// <summary> Creates all needed Databases </summary>
        private static void _CreateNeededDatabases() {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();

            string commandString =
                  @"CREATE TABLE IF NOT EXISTS users(username VARCHAR(255) PRIMARY KEY, password VARCHAR(255) NOT NULL, token VARCHAR(255) NOT NULL, coins INTEGER DEFAULT 20, realname VARCHAR(255) DEFAULT '', bio VARCHAR(255) DEFAULT '', image VARCHAR(255) DEFAULT '');"
                + @"CREATE TABLE IF NOT EXISTS ranking(rankID SERIAL PRIMARY KEY, username VARCHAR(255) REFERENCES users(username) ON DELETE CASCADE, points INTEGER DEFAULT 100, gamecount INTEGER DEFAULT 0, wins INTEGER DEFAULT 0, losses INTEGER DEFAULT 0, ties INTEGER DEFAULT 0);"
                + @"CREATE TABLE IF NOT EXISTS cards(cardID VARCHAR(255) PRIMARY KEY, cardName VARCHAR(255), cardDamage NUMERIC(5, 2));"
                + @"CREATE TABLE IF NOT EXISTS decks(deckID SERIAL PRIMARY KEY, username VARCHAR(255) REFERENCES users(username) ON DELETE CASCADE, cardID VARCHAR(255) REFERENCES cards(cardID));"
                + @"CREATE TABLE IF NOT EXISTS stacks(stackID SERIAL PRIMARY KEY, username VARCHAR(255) REFERENCES users(username) ON DELETE CASCADE, cardId VARCHAR(255) REFERENCES cards(cardID));" 
                + @"CREATE TABLE IF NOT EXISTS tradebank(tradeID VARCHAR(255) PRIMARY KEY, seller VARCHAR(255) REFERENCES users(username) ON DELETE CASCADE, cardID VARCHAR(255) REFERENCES cards(cardID), reqType VARCHAR(255), reqElement VARCHAR(255), reqDmg NUMERIC(5, 2));"
                + @"CREATE TABLE IF NOT EXISTS packs(packID VARCHAR(255) PRIMARY KEY, firstC VARCHAR(255) REFERENCES cards(cardID) ON DELETE CASCADE, secondC VARCHAR(255) REFERENCES cards(cardID) ON DELETE CASCADE, thirdC VARCHAR(255) REFERENCES cards(cardID) ON DELETE CASCADE, fourthC VARCHAR(255) REFERENCES cards(cardID) ON DELETE CASCADE, fifthC VARCHAR(255) REFERENCES cards(cardID) ON DELETE CASCADE);"
                + @"CREATE TABLE IF NOT EXISTS battleresults(battleID VARCHAR(255) PRIMARY KEY, player_one VARCHAR(255) REFERENCES users(username) ON DELETE CASCADE, player_two VARCHAR(255) REFERENCES users(username) ON DELETE CASCADE, tie BOOLEAN DEFAULT FALSE, winner VARCHAR(255) REFERENCES users(username) ON DELETE CASCADE DEFAULT '', loser VARCHAR(255) REFERENCES users(username) ON DELETE CASCADE DEFAULT '');"
                + @"CREATE TABLE IF NOT EXISTS battlerounds(roundID SERIAL PRIMARY KEY, battleID VARCHAR(255) REFERENCES battleresults(battleID), roundNum INTEGER, tie BOOLEAN DEFAULT FALSE, winner VARCHAR(255) REFERENCES users(username) ON DELETE CASCADE, loser VARCHAR(255) REFERENCES users(username) ON DELETE CASCADE, winnerCard VARCHAR(255) REFERENCES cards(cardID) ON DELETE CASCADE, loserCard VARCHAR(255) REFERENCES cards(cardID) ON DELETE CASCADE);";
            using var cmd = new NpgsqlCommand(commandString, localConnection);
            cmd.ExecuteNonQuery();
        }

        /// <summary> Checs the DB if a user Exists </summary>
        /// <param name="username"> username to search for </param>
        /// <returns> True or False if Exists or Not </returns>
        private bool _UserExists(string username) {
            bool returnBool = false;

            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("SELECT username FROM users WHERE username = @user", localConnection);
            cmd.Parameters.AddWithValue("user", username);
            cmd.Prepare();

            using var rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
                returnBool = true;
            rdr.Close();

            return returnBool;
        }

        /// <summary> Adds all Players in the DB to Program.ActivePlayers </summary>
        private void _INITIATE_USERS() {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("SELECT users.username, users.coins, ranking.points, ranking.gamecount, ranking.wins, ranking.losses, ranking.ties FROM users JOIN ranking ON users.username = ranking.username;", localConnection);
            using var rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
                while(rdr.Read())
                    Program.ActivePlayers.Add(rdr.GetString(0), new Player(rdr.GetString(0), rdr.GetInt32(1), rdr.GetInt32(2), rdr.GetInt32(3), rdr.GetInt32(4), rdr.GetInt32(5), rdr.GetInt32(6)));
            
            rdr.Close();
        }

        /// <summary> Initiates all Cards from the DB and adds them to _cardsToInitiate </summary>
        private void _INITIATE_CARDS() {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("SELECT * FROM cards;", localConnection);
            using var rdr = cmd.ExecuteReader();
            if (rdr.HasRows) {
                while (rdr.Read()) {
                    Program.AllCardIDs.Add(rdr.GetString(0));

                    if(rdr.GetString(1) == "monster")
                        _cardsToInitiate.Add(rdr.GetString(0), new MonsterCard(rdr.GetString(0), rdr.GetString(1), rdr.GetDouble(2)));
                    else 
                        _cardsToInitiate.Add(rdr.GetString(0), new SpellCard(rdr.GetString(0), rdr.GetString(1), rdr.GetDouble(2)));
                    
                }
            }

            rdr.Close();
        }

        /// <summary> Initiates all Packs from the DB and adds the mto the Shop </summary>
        private void _INITIATE_SHOP() {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("SELECT * FROM packs;", localConnection);
            using var rdr = cmd.ExecuteReader();
            if (rdr.HasRows) {
                while(rdr.Read()) { 
                    CardPackage p =new CardPackage(
                            rdr.GetString(0), 
                            _cardsToInitiate[rdr.GetString(1)], 
                            _cardsToInitiate[rdr.GetString(2)], 
                            _cardsToInitiate[rdr.GetString(3)], 
                            _cardsToInitiate[rdr.GetString(4)], 
                            _cardsToInitiate[rdr.GetString(5)], 
                                5);
                    Program.Shop.AddPackageToShop(p);
                }
            }
        }

        /// <summary> Initiates all Players Stacks from the DB </summary>
        private void _INITIATE_STACKS() {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("SELECT * FROM stacks;", localConnection);
            using var rdr = cmd.ExecuteReader();
            if (rdr.HasRows) {
                while (rdr.Read()) {
                    Program.ActivePlayers[rdr.GetString(1)].AddCardToStack( _cardsToInitiate[rdr.GetString(2)] );
                }
            }
        }

        /// <summary> Initiates all Players Decks from the DB </summary>
        private void _INITIATE_DECKS() {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("SELECT * FROM decks;", localConnection);
            using var rdr = cmd.ExecuteReader();
            if (rdr.HasRows) {
                while (rdr.Read()) {
                    Program.ActivePlayers[rdr.GetString(1)].AddCardToDeck( _cardsToInitiate[rdr.GetString(2)] );
                }
            }
        }

        /// <summary> Initiates all Battles Fought from the DB</summary>
        private  void _INITIATE_BATTLEHISTORY() {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
             using var cmd = new NpgsqlCommand("SELECT * FROM battleresults;", localConnection);
             using var rdr =  cmd.ExecuteReader();
            if (rdr.HasRows) {
                while (rdr.Read()) {
                    var bid = rdr.GetString(1);
                    var pOne = Program.ActivePlayers[rdr.GetString(1)];
                    var pTwo = Program.ActivePlayers[rdr.GetString(2)];
                    var tie = rdr.GetBoolean(3);
                    var winner = Program.ActivePlayers[rdr.GetString(4)];
                    var loser = Program.ActivePlayers[rdr.GetString(5)];
                    var thisBattle = new BattleResult(bid, pOne, pTwo, tie, winner, loser);
                    _INITIATE_BATTLEROUNDS(thisBattle);
                    pOne.BattlesFought.Add(bid, thisBattle);
                    pTwo.BattlesFought.Add(bid, thisBattle);
                }
            }
        }

        /// <summary> Initiates all BattleRounds of a BattleResult from the DB </summary>
        /// <param name="res"></param>
        private async void _INITIATE_BATTLEROUNDS(BattleResult res) {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            await using var cmd = new NpgsqlCommand("SELECT * FROM battlerounds WHERE battleid = @bid;", localConnection);
            cmd.Parameters.AddWithValue("bid", res.BattleId);
            cmd.Prepare();
            await using var rdr = await cmd.ExecuteReaderAsync();
            if (rdr.HasRows) {
                while (rdr.Read()) {
                    var roundNum = rdr.GetInt32(2);
                    var roundTie = rdr.GetBoolean(3);
                    var winner = Program.ActivePlayers[rdr.GetString(4)];
                    var loser = Program.ActivePlayers[rdr.GetString(5)];
                    var wCard = _cardsToInitiate[rdr.GetString(6)];
                    var lCard = _cardsToInitiate[rdr.GetString(7)];
                    var thisRound = new BattleRound(roundNum, winner, loser, wCard, lCard, roundTie);
                    res.BattleRounds.Add(thisRound);
                }
            }
        }

        /// <summary> Initiate all Trades stored in the DB </summary>
        private void _INITIATE_TRADEBANK() {
            using var localConnection = new NpgsqlConnection(_cs);
            localConnection.Open();
            using var cmd = new NpgsqlCommand("SELECT * FROM tradebank;", localConnection);
            using var rdr = cmd.ExecuteReader();
            if (rdr.HasRows) {
                while (rdr.Read()) {
                    Player theTrader = Program.ActivePlayers[rdr.GetString(1)];
                    ElementType elem = ElementType.None;
                    switch (rdr.GetString(4)) {
                        case "fire": elem = ElementType.Fire; break;
                        case "water": elem = ElementType.Water; break;
                        case "normal": elem = ElementType.Normal; break;
                    }
                    CardType type = CardType.None;
                    switch (rdr.GetString(4)) {
                        case "monster": type = CardType.Monster; break;
                        case "spell": type = CardType.Spell; break;
                    }
                    Program.TradeBank.RegisterTrade(rdr.GetString(0), theTrader, _cardsToInitiate[rdr.GetString(2)], type, elem, rdr.GetDouble(5));
                }
            }
        }
    }
}
