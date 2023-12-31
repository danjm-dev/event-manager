using System;
using System.Collections.Generic;
using System.Linq;

namespace DJM.EventManager
{
    internal sealed class EventService<TEventId>
    {
        private const string HandlerParamSignatureArgumentExceptionFormat = "handler: parameter signature does not match the expected signature for event ID {0}. Expected: {1}, Actual: {2}";
        
        internal readonly IDictionary<TEventId, EventData> EventTable;

        internal EventService()
        {
            EventTable = new Dictionary<TEventId, EventData>();
        }
        
        
        // the following methods will throw exceptions if you input incorrect handler params.
        
        internal void AddHandler(TEventId eventId, Delegate handler, Type[] handlerParamSignature)
        {
            if (EventTable.TryGetValue(eventId, out var eventData))
            {
                ValidateHandlerSignature(eventId, eventData.HandlerParamSignature, handlerParamSignature);
                
                if(eventData.Handlers.GetInvocationList().Contains(handler)) return;
                
                eventData.Handlers = Delegate.Combine(eventData.Handlers, handler);
                EventTable[eventId] = eventData;
            }
            else
                EventTable[eventId] = new EventData(handler, handlerParamSignature);
        }
        
        internal void RemoveHandler(TEventId eventId, Delegate handler, Type[] handlerParamSignature)
        {
            if (!EventTable.TryGetValue(eventId, out var eventData))
                return;
            
            ValidateHandlerSignature(eventId, eventData.HandlerParamSignature, handlerParamSignature);
            
            var remainingHandlers = Delegate.Remove(eventData.Handlers, handler);

            if (remainingHandlers is not null)
            {
                eventData.Handlers = remainingHandlers;
                EventTable[eventId] = eventData;
                return;
            }
            
            EventTable.Remove(eventId);
        }
        
        internal static void ValidateHandlerSignature(TEventId eventId, Type[] expectedTypes, Type[] actualTypes)
        {
            // match as no parameters for either
            if(expectedTypes is null && actualTypes is null) return;
            
            // match as neither null, and sequence equal
            if (expectedTypes != null && actualTypes != null && expectedTypes.SequenceEqual(actualTypes)) return;
            
            // no match, throw exception
            var expected = expectedTypes is null ? "null" : string.Join(", ", expectedTypes.Select(t => t.Name));
            var actual = actualTypes is null ? "null" : string.Join(", ", actualTypes.Select(t => t.Name));
                    
            throw new ArgumentException(string.Format(HandlerParamSignatureArgumentExceptionFormat, eventId, expected, actual));
        }
    }
}