-- View: fps.vtbltestrequ

CREATE OR REPLACE VIEW fps.vtbltestrequ AS
SELECT DISTINCT
    tr.buyer       AS jobcode,
    tr.testcode,
    tr.norequired  AS notests,
    tr.unitprice   AS testprice,
    tr.datecreated,
    tr.projectbuyercode,
    tr.fpsyear,
    u.user_id,
    u.dt2username,
    u.useremail
FROM fps.tlkptestreqmt tr
JOIN fps.tlkpproject pj      ON tr.buyer     = pj.parentproject
JOIN fps.tlkpprogram pg      ON pj.program   = pg.programno
JOIN fps.tbluser_program up  ON pg.programno = up.programno
JOIN fps.tblusers u          ON up.user_id   = u.user_id;
