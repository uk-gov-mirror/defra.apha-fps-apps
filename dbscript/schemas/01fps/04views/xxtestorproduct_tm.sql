-- View: fps.xxtestorproduct_tm

CREATE OR REPLACE VIEW fps.xxtestorproduct_tm AS
SELECT DISTINCT
    tp.itemcode,
    tp.itemdescription,
    tp.testmanager,
    tp.jobstatus,
    tp.unitpricevla,
    tp.priceahvg,
    tp.owner,
    tp.chargemethod,
    tp.shortdescription,
    tp.defraunitprice,
    tp.fpsyear,
    u.username,
    u.dt2username,
    u.useremail
FROM fps.testorproduct tp
JOIN fps.tbluser_testowner uto ON tp.owner   = uto.test_owner
JOIN fps.tblusers u            ON uto.user_id = u.user_id;
