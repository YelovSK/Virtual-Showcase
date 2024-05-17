import open3d as o3d
from abc import ABC, abstractmethod


class Registration(ABC):

    @abstractmethod
    def align(self):
        pass


class GlobalRegistration(Registration):

    def __init__(self, source, target, voxel_size):
        self.source = source
        self.target = target
        self.voxel_size = voxel_size

    def align(self):
        distance_threshold = self.voxel_size * 1.5

        source_down, source_fpfh = self._preprocess_point_cloud(self.source)
        target_down, target_fpfh = self._preprocess_point_cloud(self.target)

        result = o3d.pipelines.registration.registration_ransac_based_on_feature_matching(
            source_down, target_down, source_fpfh, target_fpfh, True, distance_threshold,
            o3d.pipelines.registration.TransformationEstimationPointToPoint(with_scaling=True),
            3,
            [
                o3d.pipelines.registration.CorrespondenceCheckerBasedOnEdgeLength(0.1),
                o3d.pipelines.registration.CorrespondenceCheckerBasedOnDistance(distance_threshold)
            ],
            o3d.pipelines.registration.RANSACConvergenceCriteria(100_000, 0.999)
        )

        return result

    def _preprocess_point_cloud(self, pcd):
        pcd_down = pcd.voxel_down_sample(self.voxel_size)

        radius_normal = self.voxel_size * 2
        pcd_down.estimate_normals(o3d.geometry.KDTreeSearchParamHybrid(radius=radius_normal, max_nn=30))

        radius_feature = self.voxel_size * 5
        pcd_fpfh = o3d.pipelines.registration.compute_fpfh_feature(pcd_down, o3d.geometry.KDTreeSearchParamHybrid(radius=radius_feature, max_nn=100))

        return pcd_down, pcd_fpfh


class ICPRegistration(Registration):

    def __init__(self, source, target, initial_transformation, threshold):
        self.source = source
        self.target = target
        self.initial_transformation = initial_transformation
        self.threshold = threshold

    def align(self):
        result = o3d.pipelines.registration.registration_icp(
            self.source,
            self.target,
            self.threshold,
            self.initial_transformation,
            o3d.pipelines.registration.TransformationEstimationPointToPoint(with_scaling=True),
            o3d.pipelines.registration.ICPConvergenceCriteria(
                max_iteration=1000,
                relative_fitness=1e-6,
                relative_rmse=1e-6,
            ),
        )

        return result
