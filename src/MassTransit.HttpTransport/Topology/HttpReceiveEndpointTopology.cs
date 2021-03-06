﻿// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.HttpTransport.Topology
{
    using System;
    using Configuration;
    using GreenPipes;
    using MassTransit.Pipeline;
    using MassTransit.Pipeline.Filters;
    using MassTransit.Pipeline.Observables;
    using MassTransit.Pipeline.Pipes;
    using MassTransit.Topology;
    using Microsoft.AspNetCore.Http;
    using Transport;
    using Transports;


    public class HttpReceiveEndpointTopology :
        ReceiveEndpointTopology,
        IHttpReceiveEndpointTopology
    {
        readonly IHttpReceiveEndpointConfiguration _configuration;
        readonly IConsumePipe _consumePipe;
        readonly Lazy<ISendTransportProvider> _sendTransportProvider;

        public HttpReceiveEndpointTopology(IHttpReceiveEndpointConfiguration configuration)
            : base(configuration)
        {
            _configuration = configuration;

            _consumePipe = configuration.Consume.CreatePipe();

            _sendTransportProvider = new Lazy<ISendTransportProvider>(CreateSendTransportProvider);
        }

        public IReceiveEndpointTopology CreateResponseEndpointTopology(HttpContext httpContext)
        {
            return new HttpResponseReceiveEndpointTopology(this, httpContext, SendPipe, Serializer);
        }

        protected override ISendEndpointProvider CreateSendEndpointProvider()
        {
            return new SendEndpointProvider(_sendTransportProvider.Value, SendObservers, Serializer, InputAddress, SendPipe);
        }

        protected override IPublishEndpointProvider CreatePublishEndpointProvider()
        {
            return new HttpPublishEndpointProvider(_configuration.HostAddress, Serializer, _sendTransportProvider.Value, PublishPipe);
        }

        ISendTransportProvider CreateSendTransportProvider()
        {
            IPipe<ReceiveContext> pipe = Pipe.New<ReceiveContext>(x =>
            {
                x.UseFilter(new DeserializeFilter(_configuration.Serialization.Deserializer, _consumePipe));
            });

            var receivePipe = new ReceivePipe(pipe, _consumePipe);

            return new HttpSendTransportProvider(_configuration.BusConfiguration, receivePipe, new ReceiveObservable(), this);
        }
    }
}