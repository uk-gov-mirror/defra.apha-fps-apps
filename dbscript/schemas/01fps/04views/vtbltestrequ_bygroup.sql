-- View: fps.vtbltestrequ_bygroup

CREATE OR REPLACE VIEW fps.vtbltestrequ_bygroup AS
SELECT DISTINCT
    tr.buyer       AS jobcode,
    tr.testcode,
    tr.norequired  AS notests,
    tr.unitprice   AS testprice,
    tr.datecreated,
    tr.projectbuyercode,
    tr.fpsyear,
    p.user_id,
    p.dt2username,
    p.useremail
FROM fps.tlkptestreqmt tr
JOIN fps.vtlkpproject_bygroup p ON tr.buyer = p.parentproject;
