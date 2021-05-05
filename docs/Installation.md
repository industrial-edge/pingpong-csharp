# Installation

- [Installation](#installation)
  - [Build application](#build-application)
    - [Cloning the repository](#cloning-the-repository)
    - [Build docker image](#build-docker-image)
  - [Configuring the Industrial Edge Databus](#configuring-the-industrial-edge-databus)
  - [Create configuration for the application](#create-configuration-for-the-application)
    - [Configuration via fixed config file (UseCase 1)](#configuration-via-fixed-config-file-usecase-1)
    - [Configuration via app Configuration Service (UseCase 2)](#configuration-via-app-configuration-service-usecase-2)
  - [Upload the application to the Industrial Edge Management](#upload-the-application-to-the-industrial-edge-management)
  - [Configuring and deploying the application to a Industrial Edge Device](#configuring-and-deploying-the-application-to-a-industrial-edge-device)
    - [Create a fixed configuration via file upload (UseCase 1)](#create-a-fixed-configuration-via-file-upload-usecase-1)
    - [Create a custom UI configuration (UseCase 2)](#create-a-custom-ui-configuration-usecase-2)
    - [Installing the application to an Industrial Edge Device](#installing-the-application-to-an-industrial-edge-device)
  - [Testing the application using Simatic Flow Creator](#testing-the-application-using-simatic-flow-creator)

## Build application

### Cloning the repository

- Clone or Download the source code to your engineering VM

### Build docker image

- Open a console in the source code folder
- Use command `docker-compose build` in the folder where the docker-compose.yml file is located to build the docker image
- This docker image can now be used to build your application with the Industrial Edge App Publisher

![Build docker image](./graphics/docker-compose-build.png)

- After building, use `docker images | grep pingpong_csharp` to check if the image was build correctly.

You should see a similar result to this:

![Check for docker image](./graphics/docker-images-grep.png)

## Configuring the Industrial Edge Databus

For the PingPong application the databus must provide two topics to publish and subscribe to.

- Open the Industrial Edge Management web interface
- Go to "Data Connections" > IE Databus
- Select the corresponding Industrial Edge Device
- Create a new user with username and password and give the user publish and subscribe permission
- Create two topics for the PingPong application
- Deploy the databus configuration and wait for the job to be finished successfully

![Databus](./graphics/Databus.png)

## Create configuration for the application

The following parameter must be configured for the PingPong application:

- "MQTT_USER": username of the databus user
- "MQTT_PASSWORD": password of the databus user
- "MQTT_IP": name of the databus service name - serve as DNS resolution (ie_databus / ie-databus (> V1.0))
- "TOPIC_1": databus topic to which the application subscribes to
- "TOPIC_2": databus topic to which the application publishes to

The configuration file has to be named `mqtt-config.json`.

Below two ways are described to create the configuration file.

### Configuration via fixed config file (UseCase 1)

Here a fixed configuration file is created, that can not be modified during the installation of the application. It has to be structured like the following example:

```json
{
    "MQTT_USER":"edge",
    "MQTT_PASSWORD":"edge",
    "MQTT_IP":"ie-databus",
    "TOPIC_1":"topic1",
    "TOPIC_2":"topic1"
}
```

This repository already provides that configuration file [here](./../cfg-data/mqtt-config.json).
In this example, the application will authenticate to the IE databus with the username `edge` and password `edge`. It will subscribe to `topic1` and will publish to `topic2`.

### Configuration via app Configuration Service (UseCase 2)

Here the system app IE Configuration Service is used to create an UI for the configuration of the application. The UI is based on a JSON Forms file, that is integrated as a configuration template via the Publisher. By using this configuration during the installation, the user can fill out the parameter individual.

First the system app IE Configuration Service must be installed on the IEM.

![ConfigurationService](./graphics/ConfigurationService.png)

Then a JSON Forms file must be created, consisting of an UI schema and a data schema. Please see this [getting started](https://jsonforms.io/docs/getting-started) to learn more about JSON Forms. The file should look like this:

![JsonSchema](./graphics/JsonSchema.png)

This repository already provides that JSON Forms configuration file [here](./../cfg-data/mqtt-config_schema.json). Please rename this file into `mqtt-config.json` on your host to use it properly.

## Upload the application to the Industrial Edge Management

Please refer to [Uploading App to IEM](https://github.com/industrial-edge/upload-app-to-industrial-edge-management) on how to upload the app to the IEM. In the Industrial Edge App Publisher, use the `docker-compose.yml` file of this application example when building the docker image.

## Configuring and deploying the application to a Industrial Edge Device

An application can provide several configurations, that can be selected during installation.
If no configuration is used (e.g. if the application is deployed as a standalone application), the application will use the corresponding environmental variables specified in the `docker-compose.yml` file.

### Create a fixed configuration via file upload (UseCase 1)

- Open the Industrial Edge Management web interface
- Go to "Applications" > "My Projects"
- Open the PingPong application
- Click on "Configurations" > "Add Configuration"
- Enter a name and description
- Enter `./cfg-data` as host path
- Enter a template name and description
- Browse for the `mqtt-config.json` file created [here](#configuration-via-fixed-config-file-usecase-1)
- Click "Add"

![ConfigViaFile](./graphics/ConfigViaFile.png)

### Create a custom UI configuration (UseCase 2)

- Open the Industrial Edge App Publisher
- Open the PingPong application
- Click on "Configurations" > "Add Configuration"
- Enter a name and description
- Enter `./cfg-data` as host path
- Enter a template name and description
- Browse for the `mqtt-config.json` file created [here](#configuration-via-app-configuration-service-usecase-2)
- Activate "Json Schema"
- Click "Add"

![ConfigViaUi](./graphics/ConfigViaUi.png)

### Installing the application to an Industrial Edge Device

- Open the Industrial Edge Management web interface
- Go to "Applications" > "My Projects"
- Open the PingPong application
- Click on the install button on the right of the version you want to deploy
- Under "Schema Configurations" select the above created UI configuration and fill the parameter
![Config1](./graphics/Config1.png)

OR
- Under "Other Configurations" select the above created file configuration
![Config2](./graphics/Config2.png)
- Select the corresponing Industrial Edge Device
- Click "Install Now" and wait for the job to be finished successfully

When the PingPong application is deployed and running on the Industrial Edge Device, it can be tested using the Simatic Flow Creator.

## Testing the application using Simatic Flow Creator

- Open the Web interface of the Simatic Flow Creator
- Connect a "inject" node with a "mqtt out" node
- Connect a "mqtt in" node with a "debug" node
- Configure the mqtt-nodes to connect to the databus. Enter the hostname, username and password
- Set the topics of the mqtt-nodes according to the configuration of the application. For example `topic1` as the topic to publish to and `topic2` as the topic to subscribe to.
- Deploy the flow and test by injecting a string payload into the mqtt in node. If the string is "Ping", the application will answer with "Pong". If the string is "Pong" the application will answer with "Ping".

The finished flow is available [here](./../src/Flow_Pingpong_Test.json) and can be imported into the simatic flow creator.

![PingPong test in Simatic Flow Creator](./graphics/pingpong-flowcreator.gif)
