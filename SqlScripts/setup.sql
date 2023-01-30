CREATE TABLE IF NOT EXISTS `simulationdevice` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `DeviceGuid` char(36) NOT NULL,
  `Settings` json DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `DeviceGuid` (`DeviceGuid`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `simulationjob` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `HangFireJobId` int NOT NULL,
  `DeviceGuid` char(36) NOT NULL,
  `CreatedDate` datetime NOT NULL,
  `QueuePosition` int NOT NULL,
  `UpdatedDate` datetime NOT NULL,
  `ReferenceKey` varchar(30) NOT NULL,
  `Duration` time NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=133 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;