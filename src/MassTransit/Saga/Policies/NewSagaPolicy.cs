// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Saga.Policies
{
    using System;
    using System.Threading.Tasks;
    using MassTransit.Pipeline;


    /// <summary>
    /// Accepts a message to a saga that does not already exist, throwing an exception if an existing
    /// saga instance is specified.
    /// </summary>
    /// <typeparam name="TSaga">The saga type</typeparam>
    /// <typeparam name="TMessage">The message type</typeparam>
    public class NewSagaPolicy<TSaga, TMessage> :
        ISagaPolicy<TSaga, TMessage>
        where TSaga : class, ISaga
        where TMessage : class
    {
        readonly bool _insertOnInitial;
        readonly ISagaFactory<TSaga, TMessage> _sagaFactory;

        public NewSagaPolicy(ISagaFactory<TSaga, TMessage> sagaFactory, bool insertOnInitial)
        {
            _sagaFactory = sagaFactory;
            _insertOnInitial = insertOnInitial;
        }

        public bool PreInsertInstance(ConsumeContext<TMessage> context, out TSaga instance)
        {
            if (_insertOnInitial)
            {
                instance = _sagaFactory.Create(context);
                return true;
            }

            instance = null;
            return false;
        }

        Task ISagaPolicy<TSaga, TMessage>.Existing(SagaConsumeContext<TSaga, TMessage> context, IPipe<SagaConsumeContext<TSaga, TMessage>> next)
        {
            throw new SagaException("The message cannot be accepted by an existing saga", typeof(TSaga), typeof(TMessage),
                context.CorrelationId ?? Guid.Empty);
        }

        async Task ISagaPolicy<TSaga, TMessage>.Missing(ConsumeContext<TMessage> context, IPipe<SagaConsumeContext<TSaga, TMessage>> next)
        {
            await _sagaFactory.Send(context, next).ConfigureAwait(false);
        }
    }
}