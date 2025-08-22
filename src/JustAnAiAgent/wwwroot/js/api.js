class Api {
    constructor(baseUrl) {
        this.baseUrl = baseUrl.replace(/\/+$/, '');
    }

    async buildRequest(method, path, data, options = {}) {
        const url = `${this.baseUrl}/${path.replace(/^\/+/, '')}`;

        const config = {
            method,
            headers: {
                Accept: 'application/json',
                ...(data ? { 'Content-Type': 'application/json' } : {}),
                ...options.headers,
            },
            ...options,
        };

        if (data !== undefined) {
            config.body = JSON.stringify(data);
        }

        const response = await fetch(url, config);

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`HTTP ${response.status}: ${errorText}`);
        }

        return response.json();
    }

    async get(path, options = {}) {
        return this.buildRequest('GET', path, undefined, options);
    }

    async post(path, body, options = {}) {
        return this.buildRequest('POST', path, body, options);
    }

    async put(path, body, options = {}) {
        return this.buildRequest('PUT', path, body, options);
    }

    async delete(path, options = {}) {
        return this.buildRequest('DELETE', path, undefined, options);
    }

    // -----------------------------------------------------------------------
    // File upload helper
    // -----------------------------------------------------------------------

    /**
     * POST a file (or any Blob) along with optional additional fields.
     *
     * @param {string}      path            - Endpoint relative to baseUrl
     * @param {Blob|File}   file            - The file to upload
     * @param {Object}      [fields={}]     - Additional key/value pairs to send
     * @param {Object}      [options={}]    - Optional fetch config overrides
     * @returns {Promise<any>} Parsed JSON from the response
     */
    async postWithFile(path, file, fields = {}, options = {}) {
        const url = `${this.baseUrl}/${path.replace(/^\/+/, '')}`;

        const formData = new FormData();
        formData.append('file', file);

        // Attach any extra fields (strings, numbers, etc.)
        Object.entries(fields).forEach(([key, value]) => {
            formData.append(key, value);
        });

        const config = {
            method: 'POST',
            body: formData,
            ...options,
        };

        const response = await fetch(url, config);

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`HTTP ${response.status}: ${errorText}`);
        }

        return response.json();
    }
}