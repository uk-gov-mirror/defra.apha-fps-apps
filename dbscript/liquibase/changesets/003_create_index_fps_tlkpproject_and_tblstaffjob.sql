--liquibase formatted sql

--changeset repo-admin:003_fps_index idx_tlkpproject_pg_yr_and_idx_tblstaffjob_jobcode_yr labels:ddl context: vpvtprojectgroupmgrplan


CREATE INDEX  idx_tlkpproject_pg_yr
    ON fps.tlkpproject (projectgroup, fpsyear);

CREATE INDEX  idx_tblstaffjob_jobcode_yr
    ON fps.tblstaffjob (jobcode, fpsyear);
	
--rollback ;