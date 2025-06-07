//using Microsoft.Extensions.DependencyInjection;
//using Polly;
//using Polly.Extensions.Http;

//namespace Order.Infrastructure.ExternalServices.Common;

//public static class RetryPolicyConfiguration
//{
//    public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder builder)
//    {
//        return builder.AddPolicyHandler(GetRetryPolicy());
//    }

//    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
//    {
//        return HttpPolicyExtensions
//            .HandleTransientHttpError()
//            .OrResult(msg => !msg.IsSuccessStatusCode)
//            .WaitAndRetryAsync(
//                3,
//                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
//    }
//}