-- Initialize schemas and extensions for PostgreSQL
CREATE SCHEMA IF NOT EXISTS iam;
CREATE SCHEMA IF NOT EXISTS credit;
CREATE SCHEMA IF NOT EXISTS compliance;
CREATE SCHEMA IF NOT EXISTS worker;
CREATE SCHEMA IF NOT EXISTS keycloak;

GRANT ALL ON SCHEMA iam        TO crcl;
GRANT ALL ON SCHEMA credit     TO crcl;
GRANT ALL ON SCHEMA compliance TO crcl;
GRANT ALL ON SCHEMA worker     TO crcl;
GRANT ALL ON SCHEMA keycloak   TO crcl;
