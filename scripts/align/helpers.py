import open3d as o3d
import copy
from dataclasses import dataclass
import scipy
import numpy as np

@dataclass
class Coordinates:
    x: float
    y: float
    z: float

    def __str__(self):
        return f"x: {self.x}, y: {self.y}, z: {self.z}"

@dataclass
class Transformation:
    rotation_matrix: Coordinates
    translation: Coordinates
    scale: float

    def __str__(self):
        return f"Rotation: {self.rotation_matrix}\nTranslation: {self.translation}\nScale: {self.scale}"

def decompose_transformation_matrix(transformation_matrix) -> Transformation:
    # Rotation
    rotation_matrix = transformation_matrix[:3, :3]
    rotation_matrix = scipy.linalg.orth(rotation_matrix)
    angles = scipy.spatial.transform.Rotation.from_matrix(rotation_matrix).as_euler("xyz", degrees=True)

    # Translation
    translation = transformation_matrix[:, 3][:3]

    # Scale (should be the same on all axes)
    scale_x = np.linalg.norm(transformation_matrix[:, 0])
    scale_y = np.linalg.norm(transformation_matrix[:, 1])
    scale_z = np.linalg.norm(transformation_matrix[:, 2])
    assert abs(scale_x - scale_y) < 1e-6 and abs(scale_y - scale_z) < 1e-6 # float precision

    return Transformation(
        rotation_matrix=Coordinates(x=angles[0], y=angles[1], z=angles[2]),
        translation=Coordinates(x=translation[0], y=translation[1], z=translation[2]),
        scale=scale_x
    )

def draw_registration_result(source, target, transformation, window_name="Open3D Registration Result"):
    source_temp = copy.deepcopy(source)
    target_temp = copy.deepcopy(target)
    source_temp.paint_uniform_color([1, 0.7, 0])
    target_temp.paint_uniform_color([0, 0.6, 0.9])
    source_temp.transform(transformation)
    o3d.visualization.draw_geometries([source_temp, target_temp], window_name)
