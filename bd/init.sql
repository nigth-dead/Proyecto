-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
--
-- Host: localhost    Database: punto_de_venta
-- ------------------------------------------------------
-- Server version	8.0.44

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `punto_de_venta`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `punto_de_venta` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `punto_de_venta`;

--
-- Table structure for table `categoria`
--

DROP TABLE IF EXISTS `categoria`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `categoria` (
  `categoria_id` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `descripcion` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`categoria_id`),
  UNIQUE KEY `nombre` (`nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `categoria`
--

LOCK TABLES `categoria` WRITE;
/*!40000 ALTER TABLE `categoria` DISABLE KEYS */;
INSERT INTO `categoria` VALUES (1,'Bebidas','Refrescos, jugos y agua',1),(2,'Botanas','Papas, frituras y snacks',1),(3,'Lácteos','Leche, queso y yogurt',1),(4,'Panadería','Pan y repostería',1),(5,'Limpieza','Productos de limpieza',1);
/*!40000 ALTER TABLE `categoria` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `corte_caja`
--

DROP TABLE IF EXISTS `corte_caja`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `corte_caja` (
  `corte_id` int NOT NULL AUTO_INCREMENT,
  `tienda_id` int NOT NULL,
  `usuario_id` int NOT NULL,
  `fecha_apertura` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `fecha_cierre` datetime DEFAULT NULL,
  `saldo_inicial` decimal(10,2) NOT NULL DEFAULT '0.00',
  `saldo_esperado` decimal(10,2) DEFAULT NULL,
  `saldo_final` decimal(10,2) DEFAULT NULL,
  `estado` enum('Completo','Pendiente') COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`corte_id`),
  KEY `fk_corte_usuario` (`usuario_id`),
  KEY `idx_corte_tienda_fecha` (`tienda_id`,`fecha_apertura`),
  CONSTRAINT `fk_corte_tienda` FOREIGN KEY (`tienda_id`) REFERENCES `tienda` (`tienda_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_corte_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `corte_caja`
--

LOCK TABLES `corte_caja` WRITE;
/*!40000 ALTER TABLE `corte_caja` DISABLE KEYS */;
INSERT INTO `corte_caja` VALUES (1,1,1,'2026-06-04 08:00:00','2026-06-04 18:00:00',500.00,1850.50,1850.50,'Completo'),(2,1,1,'2026-06-09 17:59:16',NULL,0.00,NULL,NULL,'Pendiente');
/*!40000 ALTER TABLE `corte_caja` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `detalle_venta`
--

DROP TABLE IF EXISTS `detalle_venta`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `detalle_venta` (
  `detalle_id` int NOT NULL AUTO_INCREMENT,
  `venta_id` int NOT NULL,
  `producto_id` int NOT NULL,
  `cantidad` int NOT NULL,
  `precio_unitario` decimal(10,2) NOT NULL,
  `subtotal` decimal(10,2) GENERATED ALWAYS AS ((`cantidad` * `precio_unitario`)) STORED,
  `iva` decimal(10,0) NOT NULL,
  PRIMARY KEY (`detalle_id`),
  KEY `fk_detalle_producto` (`producto_id`),
  KEY `idx_detalle_venta` (`venta_id`),
  CONSTRAINT `fk_detalle_producto` FOREIGN KEY (`producto_id`) REFERENCES `producto` (`producto_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_detalle_venta` FOREIGN KEY (`venta_id`) REFERENCES `venta` (`venta_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `chk_detalle_cantidad` CHECK ((`cantidad` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `detalle_venta`
--

LOCK TABLES `detalle_venta` WRITE;
/*!40000 ALTER TABLE `detalle_venta` DISABLE KEYS */;
INSERT INTO `detalle_venta` (`detalle_id`, `venta_id`, `producto_id`, `cantidad`, `precio_unitario`, `iva`) VALUES (1,21,9,4,10.00,0);
/*!40000 ALTER TABLE `detalle_venta` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `historial_movimiento`
--

DROP TABLE IF EXISTS `historial_movimiento`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `historial_movimiento` (
  `movimiento_id` int NOT NULL AUTO_INCREMENT,
  `inventario_id` int NOT NULL,
  `usuario_id` int DEFAULT NULL,
  `tipo` enum('Agregar','Eliminar','Ajuste') COLLATE utf8mb4_unicode_ci NOT NULL,
  `cantidad` int NOT NULL,
  `motivo` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `fecha` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`movimiento_id`),
  KEY `fk_movimiento_inventario` (`inventario_id`),
  KEY `fk_movimiento_usuario` (`usuario_id`),
  KEY `idx_movimiento_fecha` (`fecha`),
  CONSTRAINT `fk_movimiento_inventario` FOREIGN KEY (`inventario_id`) REFERENCES `inventario` (`inventario_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_movimiento_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `historial_movimiento`
--

LOCK TABLES `historial_movimiento` WRITE;
/*!40000 ALTER TABLE `historial_movimiento` DISABLE KEYS */;
INSERT INTO `historial_movimiento` VALUES (1,1,1,'Ajuste',2,'ReduccionDeStock','2026-05-30 13:04:27'),(2,1,1,'Ajuste',2,'ReduccionDeStock','2026-05-30 13:04:42'),(3,1,1,'Ajuste',2,'ReduccionDeStock','2026-05-30 13:15:20'),(4,1,1,'Ajuste',1,'ReduccionDeStock','2026-05-31 19:09:19'),(5,1,1,'Ajuste',2,'Caducado','2026-06-01 16:08:10'),(6,1,1,'Ajuste',20,'ReduccionDeStock','2026-06-04 09:40:02'),(7,1,1,'Ajuste',31,'d','2026-06-04 10:07:57'),(8,1,1,'Ajuste',20,'error','2026-06-04 10:16:27');
/*!40000 ALTER TABLE `historial_movimiento` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `historial_pedido`
--

DROP TABLE IF EXISTS `historial_pedido`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `historial_pedido` (
  `pedido_id` int NOT NULL AUTO_INCREMENT,
  `tienda_id` int NOT NULL,
  `proveedor_id` int NOT NULL,
  `usuario_id` int DEFAULT NULL,
  `fecha` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `monto_total` decimal(10,2) NOT NULL DEFAULT '0.00',
  `estado` enum('Pendiente','Completado','Cancelado') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Pendiente',
  PRIMARY KEY (`pedido_id`),
  KEY `fk_pedido_proveedor` (`proveedor_id`),
  KEY `fk_pedido_usuario` (`usuario_id`),
  KEY `idx_pedido_estado` (`estado`),
  KEY `fk_historial_pedido_tienda` (`tienda_id`),
  CONSTRAINT `fk_historial_pedido_tienda` FOREIGN KEY (`tienda_id`) REFERENCES `tienda` (`tienda_id`),
  CONSTRAINT `fk_pedido_proveedor` FOREIGN KEY (`proveedor_id`) REFERENCES `proveedor` (`proveedor_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_pedido_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `historial_pedido`
--

LOCK TABLES `historial_pedido` WRITE;
/*!40000 ALTER TABLE `historial_pedido` DISABLE KEYS */;
INSERT INTO `historial_pedido` VALUES (1,1,1,1,'2026-05-31 18:14:02',2.00,'Completado'),(2,1,1,1,'2026-06-01 16:05:13',20.00,'Cancelado'),(3,1,1,1,'2026-06-03 19:37:55',10.00,'Completado'),(4,1,1,1,'2026-06-03 19:45:46',100.00,'Completado'),(5,1,1,1,'2026-06-03 20:00:10',200.00,'Completado'),(6,4,1,1,'2026-06-04 09:12:49',40.00,'Completado'),(7,1,1,1,'2026-06-04 09:58:15',0.01,'Cancelado'),(8,1,1,1,'2026-06-04 10:11:43',800.00,'Completado'),(9,1,1,1,'2026-06-04 10:21:39',529.00,'Completado'),(10,1,1,1,'2026-06-04 10:26:17',529.00,'Pendiente');
/*!40000 ALTER TABLE `historial_pedido` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `historial_pedido_detalle`
--

DROP TABLE IF EXISTS `historial_pedido_detalle`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `historial_pedido_detalle` (
  `detalle_id` int NOT NULL AUTO_INCREMENT,
  `pedido_id` int NOT NULL,
  `producto_id` int NOT NULL,
  `cantidad` int NOT NULL,
  `costo_unitario` decimal(10,2) NOT NULL,
  PRIMARY KEY (`detalle_id`),
  UNIQUE KEY `uq_pedido_producto` (`pedido_id`,`producto_id`),
  KEY `fk_pedido_det_producto` (`producto_id`),
  CONSTRAINT `fk_pedido_det_pedido` FOREIGN KEY (`pedido_id`) REFERENCES `historial_pedido` (`pedido_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_pedido_det_producto` FOREIGN KEY (`producto_id`) REFERENCES `producto` (`producto_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_pedido_det_cantidad` CHECK ((`cantidad` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `historial_pedido_detalle`
--

LOCK TABLES `historial_pedido_detalle` WRITE;
/*!40000 ALTER TABLE `historial_pedido_detalle` DISABLE KEYS */;
INSERT INTO `historial_pedido_detalle` VALUES (1,1,1,2,2.00),(2,2,1,2,20.00),(3,3,1,10,10.00),(4,4,1,10,10.00),(5,5,9,10,10.00),(6,5,1,10,10.00),(7,6,1,2,20.00),(8,7,1,1,0.01),(9,8,1,20,20.00),(10,8,9,20,20.00),(11,9,1,23,23.00),(12,10,1,23,23.00);
/*!40000 ALTER TABLE `historial_pedido_detalle` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `inventario`
--

DROP TABLE IF EXISTS `inventario`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inventario` (
  `inventario_id` int NOT NULL AUTO_INCREMENT,
  `tienda_id` int NOT NULL,
  `producto_id` int NOT NULL,
  `stock` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`inventario_id`),
  UNIQUE KEY `uq_tienda_producto` (`tienda_id`,`producto_id`),
  KEY `fk_inventario_producto` (`producto_id`),
  KEY `idx_inventario_stock` (`stock`),
  CONSTRAINT `fk_inventario_producto` FOREIGN KEY (`producto_id`) REFERENCES `producto` (`producto_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_inventario_tienda` FOREIGN KEY (`tienda_id`) REFERENCES `tienda` (`tienda_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_inventario_stock` CHECK ((`stock` >= 0))
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inventario`
--

LOCK TABLES `inventario` WRITE;
/*!40000 ALTER TABLE `inventario` DISABLE KEYS */;
INSERT INTO `inventario` VALUES (1,1,1,23),(2,4,1,4),(4,1,9,20);
/*!40000 ALTER TABLE `inventario` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pago`
--

DROP TABLE IF EXISTS `pago`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pago` (
  `pago_id` int NOT NULL AUTO_INCREMENT,
  `venta_id` int NOT NULL,
  `monto` decimal(10,2) NOT NULL,
  `metodo` enum('Efectivo','Tarjeta','Transferencia') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Efectivo',
  `procesado` tinyint(1) NOT NULL DEFAULT '0',
  `fecha` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`pago_id`),
  KEY `idx_pago_venta` (`venta_id`),
  CONSTRAINT `fk_pago_venta` FOREIGN KEY (`venta_id`) REFERENCES `venta` (`venta_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_pago_monto` CHECK ((`monto` > 0))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pago`
--

LOCK TABLES `pago` WRITE;
/*!40000 ALTER TABLE `pago` DISABLE KEYS */;
/*!40000 ALTER TABLE `pago` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `producto`
--

DROP TABLE IF EXISTS `producto`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `producto` (
  `producto_id` int NOT NULL AUTO_INCREMENT,
  `proveedor_id` int DEFAULT NULL,
  `nombre` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `codigo` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `precio` decimal(10,2) NOT NULL DEFAULT '0.00',
  `activo` tinyint(1) NOT NULL DEFAULT '1',
  `categoria_id` int DEFAULT NULL,
  PRIMARY KEY (`producto_id`),
  KEY `idx_producto_proveedor` (`proveedor_id`),
  KEY `fk_producto_categoria` (`categoria_id`),
  CONSTRAINT `fk_producto_categoria` FOREIGN KEY (`categoria_id`) REFERENCES `categoria` (`categoria_id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_producto_proveedor` FOREIGN KEY (`proveedor_id`) REFERENCES `proveedor` (`proveedor_id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `chk_producto_precio` CHECK ((`precio` >= 0))
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `producto`
--

LOCK TABLES `producto` WRITE;
/*!40000 ALTER TABLE `producto` DISABLE KEYS */;
INSERT INTO `producto` VALUES (1,1,'Coca Cola 600 ml','7501055302563',20.00,1,3),(9,NULL,'papas','123',10.00,1,1);
/*!40000 ALTER TABLE `producto` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `proveedor`
--

DROP TABLE IF EXISTS `proveedor`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `proveedor` (
  `proveedor_id` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `telefono` varchar(10) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `correo` varchar(30) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`proveedor_id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `proveedor`
--

LOCK TABLES `proveedor` WRITE;
/*!40000 ALTER TABLE `proveedor` DISABLE KEYS */;
INSERT INTO `proveedor` VALUES (1,'Coca Cola FEMSA','2281234567','ventas@cocacola.com',1);
/*!40000 ALTER TABLE `proveedor` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tienda`
--

DROP TABLE IF EXISTS `tienda`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tienda` (
  `tienda_id` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `direccion` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `estado` tinyint(1) NOT NULL DEFAULT '1' COMMENT '1=activa, 0=inactiva',
  PRIMARY KEY (`tienda_id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tienda`
--

LOCK TABLES `tienda` WRITE;
/*!40000 ALTER TABLE `tienda` DISABLE KEYS */;
INSERT INTO `tienda` VALUES (1,'uno','xalapa',1),(2,'dos','veracruz',1),(4,'2','2',1);
/*!40000 ALTER TABLE `tienda` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuario`
--

DROP TABLE IF EXISTS `usuario`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuario` (
  `usuario_id` int NOT NULL AUTO_INCREMENT,
  `tienda_id` int NOT NULL,
  `nombre` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `telefono` char(10) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `contrasena` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Guardar como hash (bcrypt/argon2)',
  `rol` enum('Administrador','Cajero') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Cajero',
  `trabajando` tinyint(1) NOT NULL DEFAULT '1',
  `contratado` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`usuario_id`),
  KEY `idx_usuario_tienda` (`tienda_id`),
  CONSTRAINT `fk_usuario_tienda` FOREIGN KEY (`tienda_id`) REFERENCES `tienda` (`tienda_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuario`
--

LOCK TABLES `usuario` WRITE;
/*!40000 ALTER TABLE `usuario` DISABLE KEYS */;
INSERT INTO `usuario` VALUES (1,1,'Edgar','2282384306','123456789','Administrador',1,1),(2,1,'Emmanuel','2288314587','123456789','Cajero',1,1),(3,2,'Luis','1234567890','Exp123++','Cajero',0,1),(4,1,'Luis','1234567890','Exp123++','Cajero',0,1),(5,2,'Alejandro','2282381479','Exp123++','Administrador',0,1);
/*!40000 ALTER TABLE `usuario` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `venta`
--

DROP TABLE IF EXISTS `venta`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `venta` (
  `venta_id` int NOT NULL AUTO_INCREMENT,
  `tienda_id` int NOT NULL,
  `corte_id` int NOT NULL,
  `usuario_id` int NOT NULL,
  `fecha` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `total` decimal(10,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`venta_id`),
  KEY `fk_venta_usuario` (`usuario_id`),
  KEY `idx_venta_fecha` (`fecha`),
  KEY `idx_venta_corte` (`corte_id`),
  CONSTRAINT `fk_venta_corte` FOREIGN KEY (`corte_id`) REFERENCES `corte_caja` (`corte_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_venta_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `venta`
--

LOCK TABLES `venta` WRITE;
/*!40000 ALTER TABLE `venta` DISABLE KEYS */;
INSERT INTO `venta` VALUES (11,1,1,1,'2026-06-04 08:20:00',125.50),(12,1,1,1,'2026-06-04 09:05:00',89.00),(13,1,1,1,'2026-06-04 10:15:00',245.75),(14,1,1,1,'2026-06-04 11:30:00',60.00),(15,1,1,1,'2026-06-04 12:45:00',310.25),(16,1,1,1,'2026-06-04 13:20:00',48.50),(17,1,1,1,'2026-06-04 14:10:00',199.99),(18,1,1,1,'2026-06-04 15:35:00',75.25),(19,1,1,1,'2026-06-04 16:40:00',62.01),(20,1,1,1,'2026-06-04 17:25:00',134.25),(21,1,2,1,'2026-06-09 17:59:16',40.00);
/*!40000 ALTER TABLE `venta` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-06-10 18:41:47
