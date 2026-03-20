-- View: fps.vmilestone

CREATE OR REPLACE VIEW fps.vmilestone AS
SELECT
    m.project,
    m.milestoneref,
    m.objectiveref,
    m.milsetonetitle,
    m.plandate,
    m.actualdate,
    m.comment,
    m.monthnofin,
    m.year,
    m.fpsyear,
    p.user_id,
    p.dt2username,
    p.useremail
FROM fps.milestone m
JOIN fps.vtlkpproject p ON m.project = p.parentproject
                        AND p.fpsyear = m.fpsyear;
