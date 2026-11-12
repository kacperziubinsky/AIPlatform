"use client";

import { useEffect, useState } from "react";
import { fetchOfferDictionaries } from "@/services/dictionary-service";
import { DEFAULT_OFFER_DICTIONARIES, OfferDictionaries } from "@/types/offer";

export function useOfferDictionaries(): OfferDictionaries {
  const [dictionaries, setDictionaries] = useState<OfferDictionaries>(DEFAULT_OFFER_DICTIONARIES);

  useEffect(() => {
    let active = true;
    fetchOfferDictionaries()
      .then((data) => {
        if (active && data.categories.length > 0 && data.experiences.length > 0) setDictionaries(data);
      })
      .catch(() => {});
    return () => {
      active = false;
    };
  }, []);

  return dictionaries;
}
