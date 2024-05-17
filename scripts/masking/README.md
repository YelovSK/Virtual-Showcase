Creates masks for images in a folder. Outputs in the same folder with the ".mask.png" suffix.

# Usage

`python main.py` with the following arguments:

- `-f --folder` (str): Folder path of the images. Required.

- `-m --method` (str): Either "removebg" or "rembg". Required.

- `-t --type` (str): File extension of input images. Either ".jpg" or ".NEF". Required.

- `-o --overwrite`: Flag to overwrite existing masks (same file name).

- `-a --removebg_key` (str): API key for remove.bg. Required if method is "removebg".
