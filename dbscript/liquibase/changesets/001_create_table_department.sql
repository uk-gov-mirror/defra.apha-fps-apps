--liquibase formatted sql

--changeset repo-admin:001_new_table labels:ddl context:all
CREATE TABLE IF NOT EXISTS public.department (
    department_id BIGINT PRIMARY KEY,
    department_code VARCHAR(30) NOT NULL UNIQUE,
    department_name VARCHAR(120) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

--rollback DROP TABLE IF EXISTS public.department;
