Aligns target models with the source model. Prints the resulting transformation (scale, rotation, translation) in console.

# Usage

    parser.add_argument("-s", "--source", required=True, help="Path of the source model")
    parser.add_argument("-t","--targets", nargs="+", required=True, help="Paths of the target models")
    parser.add_argument("-v", "--voxel_size", type=float, default=DEFAULT_VOXEL_SIZE_METERS, help="Voxel size in meters")
    parser.add_argument("-n", "--vertex_count", type=int, default=DEFAULT_POINT_CLOUD_VERTEX_COUNT, help="Number of vertices in the point cloud")

`python main.py` with the following arguments:

- `-s --source` (str): Path of the polygon mesh source model. Required.

- `-t --targets` (str): Paths of the polygon mesh target models. Required.

- `-v --voxel_size` (float): Voxel size in meters for global registration.

- `-n --vertex_count` (int): Number of vertices to use in the point cloud.

# Example

    python main.py -s "source.glb" -t "target1.glb" "target2.glb" -v 0.01 -n 100000
