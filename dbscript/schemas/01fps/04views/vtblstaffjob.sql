-- View: fps.vtblstaffjob

CREATE OR REPLACE VIEW fps.vtblstaffjob AS
SELECT
    sj.staffid,
    sj.jobcode,
    sj.plannedhours,
    sj.fpsyear,
    p.user_id,
    p.dt2username,
    p.useremail
FROM fps.tblstaffjob sj
JOIN fps.vtlkpproject p ON p.parentproject = sj.jobcode
                        AND p.fpsyear      = sj.fpsyear;
