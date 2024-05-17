import os
import cv2
import rawpy
import imageio
import rembg
import argparse
from enum import Enum
from masking import Masking, RembgMasking, RemovebgMasking

MASK_SUFFIX = ".mask.png"

class ProcessType(Enum):
    REMOVEBG = "removebg"
    REMBG = "rembg"

    def __str__(self):
        return self.value

class FileType(Enum):
    NEF = ".NEF"
    JPG = ".jpg"

    def __str__(self):
        return self.value

def get_files(base_path: str, extension: str) -> list[str]:
    result = []

    for root, dirs, files in os.walk(base_path):
        for file in files:
            if file.endswith(extension) or file.endswith(extension.lower()):
                result.append(os.path.join(root, file))

    return result

def create_masks(base_path: str, masking: Masking, source_file_type: FileType, overwrite_existing: bool) -> None:
    files = get_files(base_path, str(source_file_type))

    for ix, file_path in enumerate(files):
        mask_path = file_path + MASK_SUFFIX

        if not overwrite_existing and os.path.exists(mask_path):
            print("Skipping", file_path, f"{ix + 1}/{len(files)}")
            continue

        print(file_path, f"{ix + 1}/{len(files)}")

        match source_file_type:
            # Convert RAW to JPG
            case FileType.NEF:
                jpg_path = file_path.replace(str(FileType.NEF), str(FileType.JPG))
                with rawpy.imread(file_path) as raw:
                    rgb = raw.postprocess(rawpy.Params(use_camera_wb=True))
                    imageio.imwrite(jpg_path, rgb)
            case FileType.JPG:
                jpg_path = file_path
                pass
            case _:
                raise ValueError(f"Invalid source file type: {source_file_type}")

        mask = masking.process(jpg_path)
        cv2.imwrite(mask_path, mask)

        if source_file_type == FileType.NEF:
            os.remove(jpg_path)

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("-f", "--folder", required=True, help="Folder that contains the images to process")
    parser.add_argument("-m","--method", type=ProcessType, choices=list(ProcessType), required=True, help="Method to use for masking")
    parser.add_argument("-t","--type", type=FileType, choices=list(FileType), required=True, help="Type of the images in the folder")
    parser.add_argument("-o", "--overwrite", action="store_true", help="Overwrite existing masks")
    parser.add_argument("-a", "--removebg_key", help="API key for remove.bg")
    args = vars(parser.parse_args())

    base_path = args["folder"]
    process_type = args["method"]
    image_type = args["type"]
    overwrite_existing = args["overwrite"]
    removebg_key = args["removebg_key"]

    if not os.path.exists(base_path):
        raise FileNotFoundError(f"Folder {base_path} not found")

    if not os.path.isdir(base_path):
        raise ValueError(f"{base_path} is not a folder")

    match process_type:
        case ProcessType.REMOVEBG:
            if removebg_key is None:
                raise ValueError("API_KEY is required for remove.bg masking")
            masking = RemovebgMasking(removebg_key)
        case ProcessType.REMBG:
            masking = RembgMasking(rembg.new_session(RembgMasking.DEFAULT_MODEL))
            pass
        case _:
            raise ValueError(f"Invalid process type: {process_type}")
    
    create_masks(base_path, masking, image_type, overwrite_existing)
