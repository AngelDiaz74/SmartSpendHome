window.smartSpendReceiptOcr = {
    recognizeReceipt: async function (imageDataUrl) {
        if (!imageDataUrl) {
            return {
                Success: false,
                Text: "",
                Error: "No receipt image was provided."
            };
        }

        if (!window.Tesseract) {
            return {
                Success: false,
                Text: "",
                Error: "Tesseract.js is not loaded."
            };
        }

        try {
            const result = await Tesseract.recognize(
                imageDataUrl,
                "eng",
                {
                    logger: function (message) {
                        console.log("OCR:", message);
                    }
                }
            );

            return {
                Success: true,
                Text: result?.data?.text || "",
                Error: ""
            };
        }
        catch (error) {
            console.error("Receipt OCR failed:", error);

            return {
                Success: false,
                Text: "",
                Error: error?.message || "OCR failed."
            };
        }
    }
};
