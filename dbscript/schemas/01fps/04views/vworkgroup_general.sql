-- View: fps.vworkgroup_general

CREATE OR REPLACE VIEW fps.vworkgroup_general AS
 SELECT workgroup,
    profitcentre,
    fpsyear
   FROM fps.workgroup;
