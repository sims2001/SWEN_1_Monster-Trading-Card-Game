using System.Collections.Generic;
using Newtonsoft.Json;

namespace MTCG_Rutschka {
    /// <summary> Class to Handle Trades </summary>
    public class TradeBank {

        /// <summary> Dictionary with all Registered Trades </summary>
        private Dictionary<string, Trade> _tradelist = new Dictionary<string, Trade>();

        /// <summary> Number of registered Trades </summary>
        public int TradeCount => _tradelist.Count;

        /// <summary> Returns a single Trade </summary>
        /// <param name="tid"> Trade ID </param>
        /// <exception cref="NoTradingDealException">Thrown when there is no Trade with the ID </exception>
        /// <returns> Trade Instance </returns>
        public Trade GetTrade(string tid) {
            if (_tradelist.ContainsKey(tid))
                return _tradelist[tid];
            else
                throw new NoTradingDealException("Trade does not exist");
            return null;
        }

        /// <summary> Returns a JsonList of all Trades </summary>
        /// <exception cref="NoTradingDealException">Thrown when there are no Trades </exception>
        /// <returns> JSon String with all Trades</returns>
        public string GetTrades() {
            if (_tradelist.Count == 0)
                throw new NoTradingDealException();

            List<TradingDealSchema> allTrades = new List<TradingDealSchema>();
            
            foreach (var deal in _tradelist) {
                var d = deal.Value;
                string typ = d.RequestedType == CardType.Monster ? "MONSTER" : d.RequestedType == CardType.Spell ? "SPELL" : "";
                string element = d.RequestedElement== ElementType.Fire ? "FIRE" : d.RequestedElement == ElementType.Normal ? "NORMAL" : d.RequestedElement == ElementType.Water ? "WATER" : "";
                allTrades.Add(new TradingDealSchema(deal.Key, deal.Value.OfferedCard.CardId, typ, element, d.RequestedDamage));
            }
            
            return JsonConvert.SerializeObject(allTrades);
        }

        /// <summary> Deletes a Trade with the Submittted ID </summary>
        /// <param name="tradeId"> The TradeID to delete </param>
        /// <param name="pl"> the Player requesting the deletion </param>
        /// <exception cref="NoTradingDealException">Thrown when the trade doesn't exist </exception>
        /// <exception cref="InvalidTradeCardException">Thrown when the User isnt the Trade-Vendor </exception>
        public void DeleteTrade(string tradeId, Player pl) {
            if (!_tradelist.ContainsKey(tradeId))
                throw new NoTradingDealException();
            
            if (pl == _tradelist[tradeId].Vendor) {
                pl.AddCardFromTrade(_tradelist[tradeId].OfferedCard);
                Program.MyDb.WithdrawTrade(_tradelist[tradeId]);
                _tradelist.Remove(tradeId);
            } else 
                throw new InvalidTradeCardException();
                
        }

        /// <summary> Register a new Trade </summary>
        /// <param name="id"> TradeID </param>
        /// <param name="anbieter"> Vendor </param>
        /// <param name="angebot"> Card offered </param>
        /// <param name="typ"> Requested Type </param>
        /// <param name="elem"> Requested Element </param>
        /// <param name="dmg"> Requested Dmg </param>
        /// <exception cref="TradeAlreadyExistsException"> Thrown when the Trade already exists</exception>
        public void RegisterTrade(string id, Player anbieter, ICard angebot, CardType typ, ElementType elem, double dmg) {
            if (_tradelist.ContainsKey(id))
                throw new TradeAlreadyExistsException();

            var theTrade = new Trade(id, anbieter, angebot, typ, elem, dmg);
            _tradelist.Add(id, theTrade);
            Program.MyDb.RegisterTrade(theTrade);
        }

        /// <summary> Register a Trade for Testing (No DB) </summary>
        /// <exception cref="TradeAlreadyExistsException"> Thrown when the Trade already exists</exception>
        public void RegisterTestTrade(string id, Player anbieter, ICard angebot, CardType typ, ElementType elem, double dmg) {
            if (_tradelist.ContainsKey(id))
                throw new TradeAlreadyExistsException("Trade already exists");

            var theTrade = new Trade(id, anbieter, angebot, typ, elem, dmg);
            _tradelist.Add(id, theTrade);
        }

        /// <summary> Delete Trade for Testing (No DB) </summary>
        /// <exception cref="NoTradingDealException">Thrown when the trade doesn't exist </exception>
        /// <exception cref="InvalidTradeCardException">Thrown when the User isnt the Trade-Vendor </exception>
        public void DeleteTestTrade(string tradeId, Player pl) {
            if (!_tradelist.ContainsKey(tradeId))
                throw new NoTradingDealException();

            if (pl == _tradelist[tradeId].Vendor) {
                pl.AddCardFromTrade(_tradelist[tradeId].OfferedCard);
                _tradelist.Remove(tradeId);
            }
            else
                throw new InvalidTradeCardException();

        }


        /// <summary> Execute a Trade between two Players </summary>
        /// <param name="buyer"> Buyer </param>
        /// <param name="zumVerkauf"> the Card offered </param>
        /// <param name="angebotId"> the Offer the buyer wants to trade with </param>
        /// <exception cref="NoTradingDealException">Thrown when the trade doesn't exist </exception>
        /// <exception cref="InvalidTradeCardException">Thrown when the Card doesn't meet the requirements </exception>
        /// <exception cref="SelfTradeException">Thrown when User tries to trade with itself </exception>
        public void Trade(Player buyer, ICard zumVerkauf, string angebotId) {
            if (!_tradelist.ContainsKey(angebotId))
                throw new NoTradingDealException();

            Trade offer = _tradelist[angebotId];
            if (buyer == offer.Vendor) 
                throw new SelfTradeException();

            if (_MeetsRequirements(zumVerkauf, offer)) {
                buyer.AddCardFromTrade(offer.OfferedCard);
                _tradelist.Remove(angebotId);

                offer.Vendor.AddCardFromTrade(buyer.RemoveTradeCard(zumVerkauf.CardId));
                Program.MyDb.Trade(offer, zumVerkauf, buyer);
            } else 
                throw new InvalidTradeCardException();
 
        }

        /// <summary> Checks if the Card meets the requested Requirements </summary>
        /// <param name="zTCard"> The Card to Trade </param>
        /// <param name="angTrade"> Trade Instance </param>
        /// <returns> True or False if the Card meets Requirements </returns>
        private bool _MeetsRequirements(ICard zTCard, Trade angTrade) {
            if (angTrade.RequestedElement != ElementType.None && zTCard.Element != angTrade.RequestedElement)
                return false;
            if (angTrade.RequestedType != CardType.None && zTCard.Type != angTrade.RequestedType)
                return false;
            if (angTrade.RequestedDamage > 0 && zTCard.CardDamage < angTrade.RequestedDamage)
                return false;

            return true;
        }
    }
}
