# CMPT432
A repository which shall contain all of the wonderous adventures I have in CMPT432 - Design of Compilers.

## The Illumi Compiler
The Illumi compiler is written in C# and uses the ASP.NET framework.

The current version of Illumi is command line only.  To use it, see the instructions `Building and Running Illumi`. For the future web based version of Illumi, see `Building and Running Illumi for the Web`.

## Building and Running Illumi

Illumi is a .NET console application. It is cross platform, and so can be run on Windows, Linux, and macOS.

To use Illumi, you must have the .NET core SDK installed. [This guide](https://dotnet.microsoft.com/learn/aspnet/hello-world-tutorial/intro) contains instructions for installing the .NET core.

Once you have installed .NET, follow these instructions to run Illumi on your code.

- Clone this repository
- Navigate to the `Illumi_CLI` folder
- Once inside `Illumi_CLI`, issue `dotnet run` to invoke Illumi on your code 

That's it! Easy as pie. Illumi has many more options though, to see them just issue `dotnet run -- -h` in the same place to see what else you can do with Illumi.

## Building and Running Illumi for the Web

The project is created in ASP.NET, and is interacted with through a web based front end.

To install the .NET core SDK (which includes ASP.NET) on Windows, follow the instructions given in [this guide](https://dotnet.microsoft.com/learn/aspnet/hello-world-tutorial/intro).

To install the .NET core on Linux, follow [this guide](https://dotnet.microsoft.com/download/linux-package-manager/ubuntu16-04/sdk-current). Use the drop down menu at the top to select the appropriate distro.

To build the project, first you must clone this repository to your local machine. Once you have done that, the following steps should leave you with a running version of the compiler which can be accessed by visiting `localhost:5000` or `127.0.0.1:5000`.

Build instructions:
- Ensure that you have the at least version 2.2 of the .NET core SDK installed
- Navigate to your clone of this repository, using `cd`
- Execute `dotnet run` in the root project folder, for this project that folder is called `Illumi`
- Take note of the address which the compiler is running at, it should be `localhost:5000`, but it could be different

Once you have followed these steps, you should be able to interact with the compiler and its functions by opening a web browser and navigating to the correct address.
