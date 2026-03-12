-- View: fps.vcontractadditionalcosts

CREATE OR REPLACE VIEW fps.vcontractadditionalcosts AS
 SELECT ac.jobcode,
    ac.account,
    ac.description,
    ac.itemcost,
    ac.fpsyear
   FROM fps.tbladditionalcosts ac
   JOIN fps.vcontractproject p ON p.parentproject = ac.jobcode AND p.fpsyear = ac.fpsyear;
