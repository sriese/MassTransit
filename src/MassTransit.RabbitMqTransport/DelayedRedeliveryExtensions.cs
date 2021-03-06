﻿// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit
{
    using System;
    using GreenPipes;
    using GreenPipes.Configurators;
    using PipeConfigurators;
    using RabbitMqTransport;
    using RabbitMqTransport.Configurators;
    using RabbitMqTransport.Specifications;


    public static class DelayedRedeliveryExtensions
    {
        /// <summary>
        /// Use the message scheduler to schedule redelivery of a specific message type based upon the retry policy, via
        /// the delayed exchange feature of RabbitMQ.
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="retryPolicy"></param>
        [Obsolete("Use the delegate method to configure the retry")]
        public static void UseDelayedRedelivery<T>(this IPipeConfigurator<ConsumeContext<T>> configurator, IRetryPolicy retryPolicy)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var redeliverySpecification = new DelayedExchangeRedeliveryPipeSpecification<T>();

            configurator.AddPipeSpecification(redeliverySpecification);

            var retrySpecification = new RedeliveryRetryPipeSpecification<T>();

            retrySpecification.SetRetryPolicy(x => retryPolicy);

            configurator.AddPipeSpecification(retrySpecification);
        }

        /// <summary>
        /// Use the message scheduler to schedule redelivery of a specific message type based upon the retry policy, via
        /// the delayed exchange feature of RabbitMQ.
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="configure"></param>
        public static void UseDelayedRedelivery<T>(this IPipeConfigurator<ConsumeContext<T>> configurator, Action<IRetryConfigurator> configure)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var redeliverySpecification = new DelayedExchangeRedeliveryPipeSpecification<T>();

            configurator.AddPipeSpecification(redeliverySpecification);

            var retrySpecification = new RedeliveryRetryPipeSpecification<T>();

            configure?.Invoke(retrySpecification);

            configurator.AddPipeSpecification(retrySpecification);
        }

        /// <summary>
        /// Configure delayed exchange RabbitMQ redelivery for all message types
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="configureRetry"></param>
        public static void UseDelayedRedelivery(this IRabbitMqReceiveEndpointConfigurator configurator, Action<IRetryConfigurator> configureRetry)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            if (configureRetry == null)
                throw new ArgumentNullException(nameof(configureRetry));

            configurator.ConnectConsumerConfigurationObserver(new DelayedExchangeRedeliveryConsumerConfigurationObserver(configureRetry));

            configurator.ConnectSagaConfigurationObserver(new DelayedExchangeRedeliverySagaConfigurationObserver(configureRetry));
        }
    }
}