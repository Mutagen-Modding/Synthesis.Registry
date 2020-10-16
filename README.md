![Release](https://github.com/Noggog/Synthesis.Registry/workflows/Release/badge.svg) ![Scrape](https://github.com/Noggog/Synthesis.Registry/workflows/Scrape/badge.svg)

A repository containing a listing of the known [Synthesis](https://github.com/Noggog/Synthesis) patcher repositories, as well as the scraper program to populate it.

# How it Populates
The registry is populated by leveraging GitHub's built in dependency detection systems.  You can see the list yourself [here](https://github.com/Noggog/Synthesis/network/dependents?package_id=UGFja2FnZS0xMzg1MjY1MjYz).

By scraping this list, this registry is able to automatically detect new Synthesis patchers.  It then investigates inside and looks for the extra meta files that contain description, nickname, and other information about the patcher.  

# What it Exposes
The registry then exposes the results as a simple json file.  Synthesis proper can then download this file and get an easy listing of the current state of the world.
