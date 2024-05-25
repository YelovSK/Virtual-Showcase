Aligns target models with the source model. Prints the resulting transformation (scale, rotation, translation) in console.

# Usage

`python main.py` with the following arguments:

- `-t --target` (str): Path of the polygon mesh target model. Required.

- `-s --sources` (str): Paths of the polygon mesh source models, which get aligned to the target model. Required.

- `-v --voxel_size` (float): Voxel size in meters for global registration.

- `-n --vertex_count` (int): Number of vertices to use in the point cloud.

# Example

    python main.py -t "target.glb" -s "source1.glb" "source2.glb" -v 0.01 -n 100000
