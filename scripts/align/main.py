import open3d as o3d
import numpy as np
import os
import argparse
from registration import GlobalRegistration, ICPRegistration
from helpers import draw_registration_result, decompose_transformation_matrix

DEFAULT_POINT_CLOUD_VERTEX_COUNT = 100_000
DEFAULT_VOXEL_SIZE_METERS = 0.01
DEFAULT_ICP_THRESHOLD = 0.02

def align_models(target_mesh_path, source_mesh_paths, voxel_size, vertex_count):
    target_mesh = o3d.io.read_triangle_mesh(target_mesh_path)
    target = target_mesh.sample_points_uniformly(vertex_count)

    for source_mesh_path in source_mesh_paths:
        source_mesh = o3d.io.read_triangle_mesh(source_mesh_path)
        source = source_mesh.sample_points_uniformly(vertex_count)

        global_registration = GlobalRegistration(source, target, voxel_size)
        global_registration_result = global_registration.align()

        icp_registration = ICPRegistration(source, target, global_registration_result.transformation, DEFAULT_ICP_THRESHOLD)
        icp_registration_result = icp_registration.align()

        transformation = decompose_transformation_matrix(icp_registration_result.transformation)
        print(f"{source_mesh_path} result:")
        print(transformation)

        draw_registration_result(source, target, np.identity(4), f"T: {target_mesh_path} | S: {source_mesh_path} [ORIGINAL]")
        draw_registration_result(source, target, icp_registration_result.transformation, f"T: {target_mesh_path} | S: {source_mesh_path} [ALIGNED]")

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("-t", "--target", required=True, help="Path of the target model")
    parser.add_argument("-s","--sources", nargs="+", required=True, help="Paths of the source models")
    parser.add_argument("-v", "--voxel_size", type=float, default=DEFAULT_VOXEL_SIZE_METERS, help="Voxel size in meters")
    parser.add_argument("-n", "--vertex_count", type=int, default=DEFAULT_POINT_CLOUD_VERTEX_COUNT, help="Number of vertices in the point cloud")
    args = vars(parser.parse_args())

    target_mesh_path = args["target"]
    source_mesh_paths = args["sources"]
    voxel_size = args["voxel_size"]
    vertex_count = args["vertex_count"]
    
    for path in *source_mesh_paths, target_mesh_path:
        if not os.path.exists(path):
            raise FileNotFoundError(f"File {path} not found")

    align_models(target_mesh_path, source_mesh_paths, voxel_size, vertex_count)
