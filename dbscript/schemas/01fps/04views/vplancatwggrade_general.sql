-- View: fps.vplancatwggrade_general

CREATE OR REPLACE VIEW fps.vplancatwggrade_general AS
 SELECT plancategory,
    wggrade,
    fpsyear
   FROM fps.plancatwggrade;
