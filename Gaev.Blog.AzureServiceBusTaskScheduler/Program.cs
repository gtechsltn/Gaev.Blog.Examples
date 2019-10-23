﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Microsoft.Azure.ServiceBus;

namespace Gaev.Blog.AzureServiceBusTaskScheduler
{
    class Program
    {
        const string ConnectionString = "...";

        static async Task Main(string[] args)
        {
            var cancellation = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cancellation.Cancel();
            };
            var scheduler = new AzureTaskScheduler(ConnectionString);
            await scheduler.Run(queueName: "TakeABreak", task: Process, cancellation.Token);
        }

        static async Task<Message> Process(Message message)
        {
            if (message != null)
            {
                Console.WriteLine($"Take a break! It is {message.ScheduledEnqueueTimeUtc.ToLocalTime():t}.");
                await Task.Delay(100);
            }

            return new Message
            {
                ScheduledEnqueueTimeUtc = CronExpression
                    //.Parse("*/30 8-18 * * MON-FRI")
                    .Parse("*/1 * * * MON-FRI")
                    .GetNextOccurrence(DateTime.UtcNow, TimeZoneInfo.Local)
                    .Value
            };
        }
    }
}