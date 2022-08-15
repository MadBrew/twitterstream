# TwitterStream
This console application should be deployed as a long running Azure Function where it will 
access the Twitter sample string [/2/tweets/sample/stream](https://developer.twitter.com/en/docs/twitter-api/tweets/volume-streams/api-reference/get-tweets-sample-stream).
The console app consumes the streaming API and batches the data to Azure Event Hub where it
can be accessed in a non-blocking way by other APIs.

# Demo
I can demo this app live and show the Event Hub setup in Azure where I use Azure Stream Queries
to aggregate hashtags. Another service (thus employing a decoupled microservice architecture)
would be developed to process the data that would be stored by these queries.

# Next Steps
This is just a baisc demo, to make this production ready would require a refactor (this was my first
foray into Azure Stream Analytics) to clean up the code, write better unit tests, implement integration
tests, create the CI/CD pipelines to that run these tests and then deploy to an Azure Function. A logging
service that either takes advantage of Azure Monitor/Application Insights or established telemetry/instrumentation
services like Splunk or DataDog would need to be implemented.

That being said, this sample app would be able to be easily scaled as an Azure Function and would cleave
to the principle of doing a single thing, logging the Twitter sample stream. Other services could then
access the event hub to apply whatever business logic and processes necessary to this data and display them
for a user. Such a complicated scenario of interacting services is beyond a reasonable time investment
for the purpose of this coding challenge.