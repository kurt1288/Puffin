# Puffin

Puffin is a UCI-compatible chess engine written in C#.

## Installation

Download the latest version [here](https://github.com/kurt1288/Puffin/releases/latest). Only use the legacy executable
if your CPU does not support PEXT instructions.

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