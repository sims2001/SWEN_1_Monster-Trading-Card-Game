using System;

namespace MTCG_Rutschka {

    /// <summary> Thrown when there's wrong or No Authorization for requested Action </summary>
    [Serializable]
    public class InvalidAuthorizationExceptions  : Exception {
        public InvalidAuthorizationExceptions() {}
        public InvalidAuthorizationExceptions(string message) : base(message) {}
        public InvalidAuthorizationExceptions(string message, Exception inner) : base(message, inner) {}
    }

    /// <summary> Thrown when the User already Exists </summary>
    [Serializable]
    public class UserAlreadyExistsException : Exception {
        public UserAlreadyExistsException() { }
        public UserAlreadyExistsException(string message) : base(message) { }
        public UserAlreadyExistsException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary> Thrown when the User doesn't exist </summary>
    [Serializable]
    public class UserNotFoundException : Exception {
        public UserNotFoundException() { }
        public UserNotFoundException(string message) : base(message) { }
        public UserNotFoundException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary> Thrown when the User is not the Admin </summary>
    [Serializable]
    public class NotAnAdminException : Exception {
        public NotAnAdminException() { }
        public NotAnAdminException(string message) : base(message) { }
        public NotAnAdminException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary> Wrong or No Authorization for requested Action </summary>
    [Serializable]
    public class CardAlreadyExistsException : Exception {
        public CardAlreadyExistsException() { }
        public CardAlreadyExistsException(string message) : base(message) { }
        public CardAlreadyExistsException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary> Thrown when there are no Cards </summary>
    [Serializable]
    public class NoCardsException : Exception {
        public NoCardsException() { }
        public NoCardsException(string message) : base(message) { }
        public NoCardsException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary> Thrown when the Player doesn't have enough money </summary>
    [Serializable]
    public class NotEnoughMoneyException : Exception {
        public NotEnoughMoneyException() { }
        public NotEnoughMoneyException(string message) : base(message) { }
        public NotEnoughMoneyException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary> Thrown when the CardPackage doesn't exist </summary>
    [Serializable]
    public class NoCardPackageException : Exception {
        public NoCardPackageException() { }
        public NoCardPackageException(string message) : base(message) { }
        public NoCardPackageException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary> Thrown when the Card is not Available </summary>
    [Serializable]
    public class CardNotAvailableException : Exception {
        public CardNotAvailableException() { }
        public CardNotAvailableException(string message) : base(message) { }
        public CardNotAvailableException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary> Thrown when the Wrong number of Cards is submitted </summary>
    [Serializable]
    public class WrongCardCountException : Exception {
        public WrongCardCountException() { }
        public WrongCardCountException(string message) : base(message) { }
        public WrongCardCountException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary> Thrown when the Trading Deal isn't found</summary>
    [Serializable]
    public class NoTradingDealException : Exception {
        public NoTradingDealException() { }
        public NoTradingDealException(string message) : base(message) { }
        public NoTradingDealException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary> Thrown when the Player's already in the BattleQueue </summary>
    [Serializable]
    public class AlreadyInQueueException : Exception {
        public AlreadyInQueueException() { }
        public AlreadyInQueueException(string message) : base(message) { }
        public AlreadyInQueueException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary> Thrown when the offered Card cannot be traded </summary>
    [Serializable]
    public class InvalidTradeCardException : Exception {
        public InvalidTradeCardException() { }
        public InvalidTradeCardException(string message) : base(message) { }
        public InvalidTradeCardException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary> Thrown when the User tries to trade with itself </summary>
    [Serializable]
    public class SelfTradeException : Exception {
        public SelfTradeException() { }
        public SelfTradeException(string message) : base(message) { }
        public SelfTradeException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary> Thrown when the Trade registered already exists </summary>
    [Serializable]
    public class TradeAlreadyExistsException : Exception {
        public TradeAlreadyExistsException() { }
        public TradeAlreadyExistsException(string message) : base(message) { }
        public TradeAlreadyExistsException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary> Thrown when the Input Submited does not meet the requirements </summary>
    [Serializable]
    public class InvalidInputException : Exception {
        public InvalidInputException() { }
        public InvalidInputException(string message) : base(message) { }
        public InvalidInputException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary> Thrown when a Request Route or Type is not implemented yet </summary>
    [Serializable]
    public class NotImplementedYetException : Exception {
        public NotImplementedYetException() { }
        public NotImplementedYetException(string message) : base(message) { }
        public NotImplementedYetException(string message, Exception inner) : base(message, inner) { }
    }
}
