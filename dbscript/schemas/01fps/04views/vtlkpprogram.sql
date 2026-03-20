-- View: fps.vtlkpprogram

CREATE OR REPLACE VIEW fps.vtlkpprogram AS
SELECT DISTINCT
    p.programno,
    p.programname,
    p.directorate,
    p.minim,
    p.sector_name,
    p.customer,
    p.target,
    p.manager,
    p.fpsyear,
    u.user_id,
    u.dt2username,
    u.useremail
FROM fps.tlkpprogram p
JOIN fps.tbluser_program up ON p.programno = up.programno
JOIN fps.tblusers u         ON up.user_id  = u.user_id;
