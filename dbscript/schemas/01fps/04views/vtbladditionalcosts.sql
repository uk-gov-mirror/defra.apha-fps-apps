-- View: fps.vtbladditionalcosts

CREATE OR REPLACE VIEW fps.vtbladditionalcosts AS
SELECT
    ac.jobcode,
    ac.account,
    ac.description,
    ac.itemcost,
    ac.freq,
    ac.supplier,
    ac.fpsyear,
    p.user_id,
    p.dt2username,
    p.useremail
FROM fps.tbladditionalcosts ac
JOIN fps.vtlkpproject p ON p.parentproject = ac.jobcode
                        AND p.fpsyear      = ac.fpsyear;
