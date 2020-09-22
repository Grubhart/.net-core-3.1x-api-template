using System;

namespace InnovaFunding.AuctionService
{
    public class TransactionException : Exception
    {
        public TransactionException(string message) : base(message)
        {
        }
    }
}