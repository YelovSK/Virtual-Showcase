from abc import ABC, abstractmethod
import cv2
import numpy as np
import rembg
import requests


class Masking(ABC):

    @abstractmethod
    def process(self, source_image_path) -> cv2.typing.MatLike:
        pass

class RembgMasking(Masking):
    DEFAULT_MODEL = "isnet-general-use"

    def __init__(self, session=None):
        self.session = session if session is not None else rembg.new_session("isnet-general-use")

    def process(self, source_image_path) -> cv2.typing.MatLike:
        image = cv2.imread(source_image_path)
        output = rembg.remove(image, only_mask=True, post_process_mask=True, session=self.session)

        # Threshold because of semi-transparency
        _, thresh = cv2.threshold(output, 5, 255, cv2.THRESH_BINARY)

        return thresh

class RemovebgMasking(Masking):

    def __init__(self, api_key):
        self.api_key = api_key

    def process(self, source_image_path) -> cv2.typing.MatLike:
        response = requests.post(
            "https://api.remove.bg/v1.0/removebg",
            files = {
                "image_file": open(source_image_path, "rb")
            },
            data = {
                "size": "preview",
                "channels": "alpha",
                "semitransparency": False
            },
            headers = {
                "X-Api-Key": self.api_key
            }
        )

        if response.status_code != requests.codes.ok:
            raise requests.exceptions.RequestException(f"Failed to process remove.bg request: {response.text}")

        image = np.asarray(bytearray(response.content), dtype="uint8")
        image = cv2.imdecode(image, cv2.IMREAD_GRAYSCALE)

        # Resize to original size
        original_image = cv2.imread(source_image_path)
        h, w, _ = original_image.shape
        image = cv2.resize(image, (w, h), interpolation=cv2.INTER_CUBIC)

        return image
