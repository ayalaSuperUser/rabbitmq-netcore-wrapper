 RabbitMQ .NET Core Wrapper

## Overview

This repository provides a **minimal example** demonstrating how to manage RabbitMQ connections and channels in a **.NET Core 8** application using `RabbitMQ.Client 6.4.0`.  

This setup is designed for a **multithreaded environment**, where each message is processed asynchronously.  

## Features

- Uses **RabbitMQ.Client 6.4.0**    
- Supports **parallel message processing** using `Task.Run`  
- Designed for **multithreading** and efficient **message handling**  

## Planned Update

We plan to **upgrade to RabbitMQ 4** in the near future. Due to this upcoming change, the connection has not yet been split into separate connections for publishing and consuming, as currently recommended.  

## Installation & Setup

1. Clone this repository:  

   ```sh
   git clone https://github.com/ayalaSuperUser/rabbitmq-netcore-wrapper.git
   cd rabbitmq-netcore-wrapper
   ```

2. Update RabbitMQ connection settings in appsettings.json:
  - If you have multiple RabbitMQ connections with different virtual hosts, you can configure them in appsettings.json.
  - By default, you need at least one configuration. You can define multiple connections, specifying which one is the default by setting "IsDefault": true.

```json
{
  "RabbitMqConfigurations": {
    "Configurations": [
      {
        "UserName": "user1",
        "Password": "password1",
        "HostConfiguration": {
          "HostName": "rabbitmq-host1",
          "VirtualHost": ""
        },
        "IsDefault": true
      }
  ]}
}
```

## Questions & Feedback
This repository is part of a discussion on best practices for managing RabbitMQ connections and channels in .NET Core applications.

If you have suggestions or improvements, feel free to:

- Open an issue
- Submit a pull request
- Join the discussion on the [RabbitMQ .NET Client GitHub Discussions](https://github.com/rabbitmq/rabbitmq-dotnet-client/discussions/1783)
