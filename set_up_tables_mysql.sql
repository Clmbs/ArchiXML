CREATE DEFINER=`archi`@`%` PROCEDURE `archi1`.`set_up_tables`()
begin

	-- archi1.staging definition

CREATE TABLE if not exists `staging` (
  `model_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `date` datetime NOT NULL,
  `object_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `target_table` char(20) DEFAULT NULL,
  `object_type` varchar(50) DEFAULT NULL,
  `name` varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `documentation` text,
  `source_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `target_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `subtype` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `parent_id` varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ref_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `valid_to` datetime DEFAULT NULL,
  `md5` varchar(32) DEFAULT NULL,
  KEY `staging_object_id_IDX` (`object_id`) USING BTREE,
  KEY `staging_ref_id_IDX` (`ref_id`) USING BTREE,
  KEY `staging_target_id_IDX` (`target_id`,`source_id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
	-- archi1.elements definition

CREATE TABLE if not exists `elements` (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `name` varchar(1024) DEFAULT NULL,
  `documentation` text,
  `type` varchar(1024) DEFAULT NULL,
  `valid_from` datetime DEFAULT NULL,
  `valid_to` datetime DEFAULT NULL,
  `model_id` varchar(50) DEFAULT NULL,
  `MD5` varchar(32) DEFAULT NULL,
  KEY `elements_id_IDX` (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
	
-- archi1.models definition

CREATE TABLE if not exists `models` (
  `id` varchar(50) NOT NULL,
  `name` varchar(1024) NOT NULL,
  `documentation` mediumtext,
  `title` varchar(50) DEFAULT NULL,
  `subject` varchar(30) DEFAULT NULL,
  `creator` varchar(30) DEFAULT NULL,
  `run_date` datetime DEFAULT NULL,
  `schema` varchar(50) DEFAULT NULL,
  `schema_version` varchar(50) DEFAULT NULL,
  `identifier` varchar(100) DEFAULT NULL,
  `xml_date` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- archi1.objects_in_view definition

CREATE TABLE if not exists `objects_in_view` (
  `view_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `object_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `valid_from` datetime DEFAULT NULL,
  `valid_to` datetime DEFAULT NULL,
  `model_id` varchar(50) DEFAULT NULL,
  `md5` varchar(32) DEFAULT NULL,
  KEY `objects_in_view_object_id_IDX` (`object_id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


-- archi1.properties definition

CREATE TABLE if not exists `properties` (
  `object_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `name` varchar(1024) DEFAULT NULL,
  `documentation` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `valid_from` datetime DEFAULT NULL,
  `valid_to` datetime DEFAULT NULL,
  `md5` varchar(32) DEFAULT NULL,
  `model_id` varchar(50) DEFAULT NULL,
  KEY `properties_object_id_IDX` (`object_id`) USING BTREE,
  KEY `properties_md5_IDX` (`md5`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


-- archi1.relations definition

CREATE TABLE if not exists `relations` (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `name` varchar(1024) DEFAULT NULL,
  `documentation` text,
  `type` varchar(50) DEFAULT NULL,
  `subtype` varchar(50) DEFAULT NULL,
  `valid_from` datetime DEFAULT NULL,
  `valid_to` datetime DEFAULT NULL,
  `source_id` varchar(50) DEFAULT NULL,
  `target_id` varchar(100) DEFAULT NULL,
  `md5` varchar(32) DEFAULT NULL,
  `model_id` varchar(50) DEFAULT NULL,
  KEY `relations_id_IDX` (`id`) USING BTREE,
  KEY `relations_target_id_IDX` (`target_id`,`source_id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- archi1.views definition

CREATE TABLE if not exists `views` (
  `id` varchar(50) NOT NULL,
  `model_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `folder` varchar(1024) NOT NULL,
  `name` varchar(1024) DEFAULT NULL,
  `documentation` text,
  `valid_from` datetime DEFAULT NULL,
  `valid_to` datetime DEFAULT NULL,
  `md5` varchar(32) DEFAULT NULL,
  `Created_by` varchar(30) DEFAULT NULL,
  `Created_on` varchar(30) DEFAULT NULL,
  `Last_edited_by` varchar(30) DEFAULT NULL,
  `Last_updated` varchar(30) DEFAULT NULL,
  KEY `views_md5_IDX` (`md5`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


END