Example apps to examine performance of Cosmos bulk inserts using console app and function app.

to run:
    Console:
        fill out Cosmos connection fields in the ConsolePerformance.Program.cs file
        hit f5 to run

    Function:
        fill out Cosmos connection fields in the FunctionPerformance.Startup.cs file
        hit f5 to run function
        use postman or equal app to hit the HTTP trigger in the function

Console findings
    - completes 50.000 items in 00:04:21.6263656

Function Findings
    - can't handle inserting 50.000 items. Not even a single item is inserted. Fails with "TransportException: Microsoft.Azure.Documents.TransportException: A client transport error occurred: The connection attempt timed out."
    - Will fail periodically with "Received ServiceUnavailable (Response status code does not indicate success: ServiceUnavailable (503); Substatus: 0; ActivityId: ; Reason: ();)."
      when lowering item count to 5.000.
    - completes 5.000 items in 00:12:55.812391 (with roughly 4.300 items inserted, the rest is not retried)


 