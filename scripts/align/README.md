Aligns target models with the source model. Prints the resulting transformation (scale, rotation, translation) in console.

# Usage

`python main.py` with the following arguments:

- `-s --source` (str): Path of the polygon mesh source model. Required.

- `-t --targets` (str): Paths of the polygon mesh target models. Required.

- `-v --voxel_size` (float): Voxel size in meters for global registration.

- `-n --vertex_count` (int): Number of vertices to use in the point cloud.

# Example

    python main.py -s "source.glb" -t "target1.glb" "target2.glb" -v 0.01 -n 100000
