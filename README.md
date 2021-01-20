# ResilientCommand
A Resiliency library for wrapping dependency calls. Heavily inspired by Hystrix


The flow is simple
We wrap execution in a timeout (... timeout)
We then wrap that in a circuitbreaker (Failure Threshhold, over duration of time.)
and further wrap that in a semaphore (maxParalellism)


Some future developments
* Inject ILogger
* Metrics for failures.
