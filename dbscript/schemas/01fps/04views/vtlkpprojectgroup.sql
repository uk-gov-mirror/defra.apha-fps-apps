-- View: fps.vtlkpprojectgroup

CREATE OR REPLACE VIEW fps.vtlkpprojectgroup AS
SELECT DISTINCT
    pg.projectgroup,
    u.user_id,
    u.dt2username,
    u.useremail
FROM fps.tlkpprojectgroup pg
JOIN fps.tbluser_projectgroup upg ON pg.projectgroup = upg.projectgroup
JOIN fps.tblusers u               ON upg.user_id = u.user_id;
