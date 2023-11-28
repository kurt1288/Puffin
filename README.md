# Puffin

<div align="center">
	<img src="puffin_logo.jpg" width="300" />
</div>

Puffin is a UCI-compatible chess engine written in C#.

## Installation

Download the latest version [here](https://github.com/kurt1288/Puffin/releases/latest). Only use the legacy executable
if your CPU does not support PEXT instructions.

If you want to compile it yourself, the only prerequisite is the [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0).

## Usage

Puffin is a command-line application and does not provide any user interface. Popular free GUI programs are:

* [Cute Chess](https://github.com/cutechess/cutechess)
* [Arena](http://www.playwitharena.de/)
* [Banksia GUI](https://banksiagui.com/)

Follow the instructions for the GUI on how to add a new engine.

Or you can play against it on [lichess](https://lichess.org/@/PuffinBot).

## Features

A non-exhaustive list...

Search:
* NegaScout
* Iterative deepening
* Aspiration window
* Quiescence
* Staged move generation
* PEXT sliding piece move generation
	* Legacy uses [Kindergarten Super SISSY Bitboards (KiSS)](https://www.talkchess.com/forum3/viewtopic.php?f=7&t=81234&start=30)
* Late move reductions
* Null move pruning
* Futility pruning
* Reverse futility pruning
* Transposition table (always replace scheme)

Evaluation:
* Piece square tables
* Mobility

Evaluation values have been tuned using the lichess-big3-resolved dataset.

## Special thanks to...
* [Chess Programming Wiki](https://www.chessprogramming.org/Main_Page)
* The Engine Programming Discord channel and its many users
* The [talkchess forums](https://talkchess.com)
* The [texel tuner](https://github.com/GediminasMasaitis/texel-tuner) made by Gedas
* Other open-source chess engines. Some that I have looked at and used for testing:
	* [Halogen](https://github.com/KierenP/Halogen/tree/master)
	* [Leorik](https://github.com/lithander/Leorik)
	* [Nalwald](https://gitlab.com/tsoj/Nalwald)
	* [Frozenight](https://github.com/MinusKelvin/frozenight)
	* [Princhess](https://github.com/princesslana/princhess/)
	* [Polaris](https://github.com/Ciekce/Polaris)
	* [Peacekeeper](https://github.com/Sazgr/peacekeeper)
	* [Pedantic](https://github.com/JoAnnP38/Pedantic)
	* [BlackCore](https://github.com/SzilBalazs/BlackCore/tree/master)