--liquibase formatted sql

--changeset repo-admin:003_modify_column labels:ddl context:all

ALTER TABLE public.department
ALTER COLUMN department_name TYPE VARCHAR(50),
ALTER COLUMN department_name SET NOT NULL;

--rollback ALTER TABLE public.department ALTER COLUMN department_name TYPE VARCHAR(120), ALTER COLUMN department_name SET NOT NULL;