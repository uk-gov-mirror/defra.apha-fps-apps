-- View: fps.vtblcontract

CREATE OR REPLACE VIEW fps.vtblcontract AS
SELECT DISTINCT
    c.contractno,
    c.category,
    c.manager,
    c.customer,
    c.title,
    c.registereddate,
    c.startdate,
    c.enddate,
    c.contractdoc,
    c.duration,
    c.fpsyear,
    u.dt2username,
    u.useremail
FROM fps.tblcontract c
JOIN fps.tbluser_category uc ON c.category = uc.category
JOIN fps.tblusers u          ON uc.user_id = u.user_id;
