## SSLCommerz
> SSLCommerz integration in ASP.NET Core

[![Build Status](https://travis-ci.com/shibbir/sslcommerz.svg?branch=master)](https://travis-ci.com/shibbir/sslcommerz)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](http://opensource.org/licenses/MIT)

## Instructions
You need to create a free sandbox account from [SSLCommerz](https://developer.sslcommerz.com/registration/). After signing up you will get your store id and store password via email.

In this demo project I've only used the parameters that are absolutely required to work with SSLCommerz api version 4.00. The official documentation provide much more details regarding the processes involved as well as explanation of each parameters. You should read the entire documentation for better understanding. You can find the link to the documentation in [here](https://developer.sslcommerz.com/doc/v4/).

## Running inside a docker container
You need to have [docker](https://www.docker.com/) installed on your machine before running the followings:

```bash
$ docker pull shibbir/sslcommerz
$ docker run -d --rm -p 8080:80 --name sslcommerz -e StoreId='<your_store_id>' -e StorePassword='your_store_password' sslcommerz-aspnetcore
```

## Running in Visual Studio
Open `appsettings.json` and add your store id and store password in `EnvironmentVariables` section.

```json
"EnvironmentVariables": {
    "StoreId": "your_store_id",
    "StorePassword": "your_store_password"
}
```

## Environment Variables

Name | Description
------------ | -------------
StoreId | Your SSLCommerz Store ID
StorePassword | Your SSLCommerz Store Password

## Demo
https://sslcommerz.herokuapp.com/

## License
<a href="https://opensource.org/licenses/MIT">The MIT License</a>
