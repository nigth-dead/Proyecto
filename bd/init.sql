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
  UNIQUE KEY `uq_categoria_nombre` (`nombre`)
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
  `estado` enum('Completo','Pendiente') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Pendiente',
  PRIMARY KEY (`corte_id`),
  KEY `idx_corte_tienda_fecha` (`tienda_id`,`fecha_apertura`),
  KEY `idx_corte_usuario` (`usuario_id`),
  CONSTRAINT `fk_corte_tienda` FOREIGN KEY (`tienda_id`) REFERENCES `tienda` (`tienda_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_corte_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `corte_caja`
--

LOCK TABLES `corte_caja` WRITE;
/*!40000 ALTER TABLE `corte_caja` DISABLE KEYS */;
INSERT INTO `corte_caja` VALUES (1,2,2,'2026-06-19 22:39:04','2026-06-19 22:41:04',0.00,63.80,63.80,'Completo'),(2,2,2,'2026-06-19 23:45:19','2026-06-19 23:53:09',0.00,155.20,155.20,'Completo'),(3,2,2,'2026-06-20 09:18:24','2026-06-20 09:19:15',0.00,63.80,60.00,'Completo'),(4,2,2,'2026-06-20 09:46:20','2026-06-20 09:46:57',0.00,63.80,60.00,'Completo'),(5,2,2,'2026-06-20 09:47:50','2026-06-20 09:47:58',0.00,0.00,0.00,'Completo'),(6,3,2,'2026-06-20 09:48:07',NULL,0.00,5.52,NULL,'Pendiente');
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
  `iva` decimal(10,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`detalle_id`),
  KEY `idx_detalle_venta` (`venta_id`),
  KEY `idx_detalle_producto` (`producto_id`),
  CONSTRAINT `fk_detalle_producto` FOREIGN KEY (`producto_id`) REFERENCES `producto` (`producto_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_detalle_venta` FOREIGN KEY (`venta_id`) REFERENCES `venta` (`venta_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `chk_detalle_cantidad` CHECK ((`cantidad` > 0)),
  CONSTRAINT `chk_detalle_iva` CHECK ((`iva` >= 0)),
  CONSTRAINT `chk_detalle_precio` CHECK ((`precio_unitario` >= 0))
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `detalle_venta`
--

LOCK TABLES `detalle_venta` WRITE;
/*!40000 ALTER TABLE `detalle_venta` DISABLE KEYS */;
INSERT INTO `detalle_venta` (`detalle_id`, `venta_id`, `producto_id`, `cantidad`, `precio_unitario`, `iva`) VALUES (1,1,1,1,55.00,8.80),(2,2,1,1,55.00,8.80),(3,3,1,1,55.00,8.80),(4,4,1,1,55.00,8.80),(5,5,1,1,55.00,8.80),(6,6,1,1,55.00,8.80),(7,7,1,1,55.00,8.80),(8,8,1,1,55.00,8.80),(9,9,2,1,22.00,3.52);
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
  `tipo` enum('Entrada','Ajuste','Venta') COLLATE utf8mb4_unicode_ci NOT NULL,
  `cantidad` int NOT NULL,
  `motivo` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `fecha` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`movimiento_id`),
  KEY `idx_movimiento_inventario` (`inventario_id`),
  KEY `idx_movimiento_usuario` (`usuario_id`),
  KEY `idx_movimiento_fecha` (`fecha`),
  CONSTRAINT `fk_movimiento_inventario` FOREIGN KEY (`inventario_id`) REFERENCES `inventario` (`inventario_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_movimiento_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `chk_movimiento_cantidad` CHECK ((`cantidad` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `historial_movimiento`
--

LOCK TABLES `historial_movimiento` WRITE;
/*!40000 ALTER TABLE `historial_movimiento` DISABLE KEYS */;
INSERT INTO `historial_movimiento` VALUES (1,1,1,'Entrada',20,'Pedido 1 completado','2026-06-19 22:38:28'),(2,1,2,'Venta',1,'Venta 1','2026-06-19 22:39:24'),(3,1,1,'Ajuste',2,'caduco','2026-06-19 22:40:33'),(4,1,2,'Venta',1,'Venta 2','2026-06-19 23:45:43'),(5,1,2,'Venta',1,'Venta 3','2026-06-19 23:48:41'),(6,1,2,'Venta',1,'Venta 4','2026-06-19 23:50:33'),(7,1,2,'Venta',1,'Venta 5','2026-06-19 23:52:17'),(8,1,2,'Venta',1,'Venta 6','2026-06-19 23:52:51'),(9,1,2,'Venta',1,'Venta 7','2026-06-20 09:19:07'),(10,1,2,'Venta',1,'Venta 8','2026-06-20 09:46:37'),(11,2,1,'Entrada',5,'Pedido 2 completado','2026-06-20 09:49:28'),(12,2,2,'Venta',1,'Venta 9','2026-06-20 09:49:54');
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
  KEY `idx_pedido_tienda` (`tienda_id`),
  KEY `idx_pedido_proveedor` (`proveedor_id`),
  KEY `idx_pedido_usuario` (`usuario_id`),
  KEY `idx_pedido_estado` (`estado`),
  CONSTRAINT `fk_pedido_proveedor` FOREIGN KEY (`proveedor_id`) REFERENCES `proveedor` (`proveedor_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_pedido_tienda` FOREIGN KEY (`tienda_id`) REFERENCES `tienda` (`tienda_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_pedido_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `chk_pedido_monto` CHECK ((`monto_total` >= 0))
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `historial_pedido`
--

LOCK TABLES `historial_pedido` WRITE;
/*!40000 ALTER TABLE `historial_pedido` DISABLE KEYS */;
INSERT INTO `historial_pedido` VALUES (1,2,2,1,'2026-06-19 22:38:18',1000.00,'Completado'),(2,3,1,1,'2026-06-20 09:49:02',100.00,'Completado');
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
  KEY `idx_pedido_det_producto` (`producto_id`),
  CONSTRAINT `fk_pedido_det_pedido` FOREIGN KEY (`pedido_id`) REFERENCES `historial_pedido` (`pedido_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_pedido_det_producto` FOREIGN KEY (`producto_id`) REFERENCES `producto` (`producto_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_pedido_det_cantidad` CHECK ((`cantidad` > 0)),
  CONSTRAINT `chk_pedido_det_costo` CHECK ((`costo_unitario` >= 0))
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `historial_pedido_detalle`
--

LOCK TABLES `historial_pedido_detalle` WRITE;
/*!40000 ALTER TABLE `historial_pedido_detalle` DISABLE KEYS */;
INSERT INTO `historial_pedido_detalle` VALUES (1,1,1,20,50.00),(2,2,2,5,20.00);
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
  KEY `idx_inventario_tienda` (`tienda_id`),
  KEY `idx_inventario_producto` (`producto_id`),
  KEY `idx_inventario_stock` (`stock`),
  CONSTRAINT `fk_inventario_producto` FOREIGN KEY (`producto_id`) REFERENCES `producto` (`producto_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_inventario_tienda` FOREIGN KEY (`tienda_id`) REFERENCES `tienda` (`tienda_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_inventario_stock` CHECK ((`stock` >= 0))
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inventario`
--

LOCK TABLES `inventario` WRITE;
/*!40000 ALTER TABLE `inventario` DISABLE KEYS */;
INSERT INTO `inventario` VALUES (1,2,1,10),(2,3,2,4);
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
  `metodo` enum('Efectivo','Tarjeta') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Efectivo',
  PRIMARY KEY (`pago_id`),
  KEY `idx_pago_venta` (`venta_id`),
  CONSTRAINT `fk_pago_venta` FOREIGN KEY (`venta_id`) REFERENCES `venta` (`venta_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_pago_monto` CHECK ((`monto` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pago`
--

LOCK TABLES `pago` WRITE;
/*!40000 ALTER TABLE `pago` DISABLE KEYS */;
INSERT INTO `pago` VALUES (1,1,63.80,'Efectivo'),(2,2,33.80,'Efectivo'),(3,2,30.00,'Tarjeta'),(4,3,13.80,'Efectivo'),(5,3,50.00,'Tarjeta'),(6,4,63.80,'Tarjeta'),(7,5,63.80,'Efectivo'),(8,6,43.80,'Efectivo'),(9,6,20.00,'Tarjeta'),(10,7,63.80,'Efectivo'),(11,8,63.80,'Efectivo'),(12,9,5.52,'Efectivo'),(13,9,20.00,'Tarjeta');
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
  `precio_compra` decimal(10,2) NOT NULL DEFAULT '0.00',
  `precio_venta` decimal(10,2) DEFAULT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT '1',
  `categoria_id` int DEFAULT NULL,
  PRIMARY KEY (`producto_id`),
  UNIQUE KEY `uq_producto_codigo` (`codigo`),
  KEY `idx_producto_proveedor` (`proveedor_id`),
  KEY `idx_producto_categoria` (`categoria_id`),
  CONSTRAINT `fk_producto_categoria` FOREIGN KEY (`categoria_id`) REFERENCES `categoria` (`categoria_id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_producto_proveedor` FOREIGN KEY (`proveedor_id`) REFERENCES `proveedor` (`proveedor_id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `chk_producto_precio_compra` CHECK ((`precio_compra` >= 0)),
  CONSTRAINT `chk_producto_precio_venta` CHECK (((`precio_venta` is null) or (`precio_venta` > 0)))
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `producto`
--

LOCK TABLES `producto` WRITE;
/*!40000 ALTER TABLE `producto` DISABLE KEYS */;
INSERT INTO `producto` VALUES (1,2,'Pan','123',50.00,55.00,1,4),(2,1,'Coca Cola 600ml','468',20.00,22.00,1,1);
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
  `correo` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`proveedor_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `proveedor`
--

LOCK TABLES `proveedor` WRITE;
/*!40000 ALTER TABLE `proveedor` DISABLE KEYS */;
INSERT INTO `proveedor` VALUES (1,'Coca Cola FEMSA','1234567890','ventas@cocacola.com',1),(2,'Bimbo','1234567890','bimbo@gmail.com',1);
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
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tienda`
--

LOCK TABLES `tienda` WRITE;
/*!40000 ALTER TABLE `tienda` DISABLE KEYS */;
INSERT INTO `tienda` VALUES (2,'uno','Xalapa',1),(3,'dos','Coatepec',1);
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
  `nombre` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `telefono` char(10) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `contrasena` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Guardar como hash',
  `rol` enum('Administrador','Cajero') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Cajero',
  `trabajando` tinyint(1) NOT NULL DEFAULT '0',
  `contratado` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`usuario_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuario`
--

LOCK TABLES `usuario` WRITE;
/*!40000 ALTER TABLE `usuario` DISABLE KEYS */;
INSERT INTO `usuario` VALUES (1,'Edgar','1234567890','AQAAAAIAAYagAAAAEJMP2PxXv8Ls8KtRIn/Bn5U0J9t55ELNdwKmdqBuFGiZLQXZWfoCRuZSZzQqhmwdBg==','Administrador',1,1),(2,'Emmanuel','1234567890','AQAAAAIAAYagAAAAEPEdZT5vGiSBPDOr2hdjs5yr05cKn5/sURhUXHIik+he1ouIctIohw9wn8HpA3Q61A==','Cajero',1,1);
/*!40000 ALTER TABLE `usuario` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuario_tienda`
--

DROP TABLE IF EXISTS `usuario_tienda`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuario_tienda` (
  `usuario_tienda_id` int NOT NULL AUTO_INCREMENT,
  `usuario_id` int NOT NULL,
  `tienda_id` int NOT NULL,
  PRIMARY KEY (`usuario_tienda_id`),
  UNIQUE KEY `uq_usuario_tienda` (`usuario_id`,`tienda_id`),
  KEY `idx_usuario_tienda_usuario` (`usuario_id`),
  KEY `idx_usuario_tienda_tienda` (`tienda_id`),
  CONSTRAINT `fk_usuario_tienda_tienda` FOREIGN KEY (`tienda_id`) REFERENCES `tienda` (`tienda_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_usuario_tienda_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuario_tienda`
--

LOCK TABLES `usuario_tienda` WRITE;
/*!40000 ALTER TABLE `usuario_tienda` DISABLE KEYS */;
INSERT INTO `usuario_tienda` VALUES (1,2,2),(2,2,3);
/*!40000 ALTER TABLE `usuario_tienda` ENABLE KEYS */;
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
  KEY `idx_venta_tienda` (`tienda_id`),
  KEY `idx_venta_corte` (`corte_id`),
  KEY `idx_venta_usuario` (`usuario_id`),
  KEY `idx_venta_fecha` (`fecha`),
  CONSTRAINT `fk_venta_corte` FOREIGN KEY (`corte_id`) REFERENCES `corte_caja` (`corte_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_venta_tienda` FOREIGN KEY (`tienda_id`) REFERENCES `tienda` (`tienda_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_venta_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `venta`
--

LOCK TABLES `venta` WRITE;
/*!40000 ALTER TABLE `venta` DISABLE KEYS */;
INSERT INTO `venta` VALUES (1,2,1,2,'2026-06-19 22:39:24',63.80),(2,2,2,2,'2026-06-19 23:45:43',63.80),(3,2,2,2,'2026-06-19 23:48:41',63.80),(4,2,2,2,'2026-06-19 23:50:33',63.80),(5,2,2,2,'2026-06-19 23:52:17',63.80),(6,2,2,2,'2026-06-19 23:52:51',63.80),(7,2,3,2,'2026-06-20 09:19:07',63.80),(8,2,4,2,'2026-06-20 09:46:37',63.80),(9,3,6,2,'2026-06-20 09:49:54',25.52);
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

-- Dump completed on 2026-06-20 20:24:34
