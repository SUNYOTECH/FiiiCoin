# FiiiCoin

An easy understanding and customizable blockchain framework based on layered architecture design pattern.

## Abstract

FiiiCoin is a P2P payment network specifically designed for mobile payment and mobile device mining purpose. FiiiCoin allow all mobile devices participate in together maintaining the blockchain network while leaving it idle and charging the battery, instead of requiring expensive computer hardware running 24/7 to perform the block validation work. The main objective is to create a very least effort way to maintain the blockchain network at the same time promoting the reusability of existing resources especially mobile devices. So that even the non-tech savvy user or normal consumer can easily learn and involve in mining, as long as they at least have a smartphone.

In fact, we all know that smartphone or any other mobile devices are not suitable to be setup as a blockchain node. The reasons are the computing power is weak, storage capacity is limited to keep the full blockchain data and the battery powered device are not meant to be 24/7 running all the time, and it requires high network bandwidth to synchronize the blocks data. If a mobile device runs the node using 4G network, it will finish up all the bandwidth in no time.

FiiiCoin is built using an enhanced version of Proof-of-Capacity consensus algorithm which is Delegate Proof-of-Capacity (DPoC) to achieve the mobile mining capability while keeping the mobile device from being a full node and perform extensive block synchronization work. FiiiCoin is developed using an in-house built FiiiLab proprietary customizable blockchain technology - FiiiChain. FiiiChain provide standard blockchain modules with the “plug-and-play” capability for developers to modify the blockchain characteristic base on the business requirement.

For more information, please visit https://fiii.io

## License

FiiiCoin is released under the terms of the MIT license. Please refer to https://opensource.org/licenses/MIT.

## Pre-requisite

[.NET Core 2.1](https://www.microsoft.com/net/download/dotnet-core/2.1)

## Recommended IDE Tool

[Visual Studio Community 2017](https://visualstudio.microsoft.com/)

[Visual Studio Code](https://visualstudio.microsoft.com/)

## Specification

Coin Name: FiiiCoin

Ticker Symbol: FIII

Programming Language: C#

Digital Signature Algorithm: Ed25519

Consensus Algorithm: Delegate Proof of Capacity

Supply Limit: 4,999,999,999.92724000

Subunits: 1/100000000

Block Time: 60 seconds

Block Size: 12MB

Block Reward: 250 FIII

Reward Halving: 5,000,000 blocks

Hash Function: SHA3-256

Database: SQLite

# Getting Started

For beginner:

1. Download and install the wallet from Release

2. Run the *.exe executable with administrator rights

For expert:

1. Open the project with Visual Studio

2. Run the node by executing the FiiiChain.Node which is located at the Node\Presentation folder

3. Run the wallet by executing the FiiiCoin.Wallet.Win in the Wallet folder

4. Run the mining application by executing the FiiiCoin.Miner in the Wallet folder

# Mining

Mining is not supported in PC version software for Delegate Proof of Capacity consensus. You need to purchase FiiiPOS terminal which contain a serial number registered in our FiiiCoin blockchain to be able to mine FiiiCoin.
  
# Resources

[Yellow Paper](https://fiii.io/images/doc/fiiicoin.yellowpaper.v01.pdf)

[White Paper](https://fiii.io/images/doc/whitepaper.pdf)

[Official Website](https://fiii.io)

