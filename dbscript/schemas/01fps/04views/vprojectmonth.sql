-- View: fps.vprojectmonth

CREATE OR REPLACE VIEW fps.vprojectmonth AS
SELECT
    pm.project,
    pm.monthno,
    pm.costprofile,
    pm.fpsyear,
    p.user_id,
    p.dt2username,
    p.useremail
FROM fps.projectmonth pm
JOIN fps.vtlkpproject p ON pm.project = p.parentproject
                        AND p.fpsyear = pm.fpsyear;
