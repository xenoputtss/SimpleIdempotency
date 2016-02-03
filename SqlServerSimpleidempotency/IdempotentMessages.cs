using System;
using System.Collections.Generic;

namespace SqlServerSimpleidempotency
{
    public class IdempotentMessages
    {
        private readonly HashSet<string> _processedItems = new HashSet<string>();

        public IdempotentMessages()
        {
            //TODO:  Currently this is an inmemory store.  It would be nice to persist this to a central database
            //using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //{
            //    var connectionString = "";
            //    using (var conn = new SqlConnection(connectionString))
            //    {
            //        conn.Open();
            //        using (var cmd = new SqlCommand("", conn))
            //        {
            //            cmd.CommandText = @"
            //            insert into ProcessedMessages(MessageId)
            //            select @MessageId
            //        ";
            //            cmd.CommandType = CommandType.Text;
            //            cmd.Parameters.AddWithValue("@MessageId", messageId);
            //            var r = cmd.ExecuteNonQuery();
            //            if (r != 1)
            //                return false;
            //        }
            //    }
            //    tx.Complete();
            //}
        }

        public void UnMarkMessageAsProcessed(string messageId)
        {
            lock (_processedItems)
                _processedItems.Remove(messageId);
        }

        public void MarkMessageAsProcessed(string messageId)
        {
            lock (_processedItems)
                _processedItems.Add(messageId);
        }

        public bool HasMessageBeenProcessed(string messageId)
        {
            lock (_processedItems)
                return _processedItems.Contains(messageId);
        }

        /// <summary>
        /// Handles action idempotently.
        /// </summary>
        /// <param name="uniqueId">A unique identifier that is specific to the work being done</param>
        /// <param name="action">The action to be done</param>
        public void HandleIdempotently(string uniqueId, Action action)
        {
            try
            {
                if (HasMessageBeenProcessed(uniqueId))
                    return;

                action();

                MarkMessageAsProcessed(uniqueId);
            }
            catch
            {
                UnMarkMessageAsProcessed(uniqueId);
                throw;
            }
        }

    }

}

