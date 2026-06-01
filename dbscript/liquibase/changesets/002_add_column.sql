--liquibase formatted sql

--changeset repo-admin:002_add_column labels:ddl context:all
ALTER TABLE public.department
ADD COLUMN IF NOT EXISTS status VARCHAR(20) DEFAULT 'ACTIVE';

--rollback ALTER TABLE public.department DROP COLUMN IF EXISTS status;
